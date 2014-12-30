#region

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Exceptions;
using Audiotica.Core.Utilities;
using Audiotica.Data.Service.Interfaces;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.Data.Service.RunTime
{
    public class ScrobblerService : IScrobblerService
    {
        private LastAuth _auth;
        private readonly AlbumApi _albumApi;
        private readonly ArtistApi _artistApi;
        private readonly ChartApi _chartApi;
        private readonly TrackApi _trackApi;
        private readonly UserApi _userApi;

        public ScrobblerService()
        {
            _auth = new LastAuth(ApiKeys.LastFmId, ApiKeys.LastFmSecret);
            _albumApi = new AlbumApi(_auth);
            _artistApi = new ArtistApi(_auth);
            _chartApi = new ChartApi(_auth);
            _trackApi = new TrackApi(_auth);
            _userApi = new UserApi(_auth);
            GetSessionTokenAsync();
        }

        public event EventHandler<bool> AuthStateChanged;

        protected virtual void OnAuthStateChanged()
        {
            EventHandler<bool> handler = AuthStateChanged;
            if (handler != null) handler(this, IsAuthenticated);
        }

        private async Task<bool> GetSessionTokenAsync()
        {
            var creds = CredentialHelper.GetCredentials("lastfm");

            if (creds == null) return false;

            var result = await GetSessionTokenWithResultsAsync(creds.UserName, creds.Password);

            if (result == LastFmApiError.BadAuth)
            {
                Logout();
                CurtainPrompt.ShowError("AuthBadCredentials".FromLanguageResource(), "Last.FM");
            }
            else 
                OnAuthStateChanged();

            return result == LastFmApiError.None;
        }

        private async Task<bool> GetSessionTokenAsync(string username, string password)
        {
            var response = await _auth.GetSessionTokenAsync(username, password);
            OnAuthStateChanged();
            return response.Success;
        }

        private async Task<LastFmApiError> GetSessionTokenWithResultsAsync(string username, string password)
        {
            var response = await _auth.GetSessionTokenAsync(username, password);
            return response.Error;
        }

        public bool HasCredentials
        {
            get { return CredentialHelper.GetCredentials("lastfm") != null; }
        }

        public bool IsAuthenticated { get { return _auth.Authenticated; } }

        public void Logout()
        {
            CredentialHelper.DeleteCredentials("lastfm");
            _auth = new LastAuth(ApiKeys.LastFmId, ApiKeys.LastFmSecret);
            OnAuthStateChanged();
        }

        public async Task<LastFmApiError> ScrobbleNowPlayingAsync(string name, string artist, DateTime played, TimeSpan duration, string album = "",
            string albumArtist = "")
        {
            if (!_auth.Authenticated)
                if (!await GetSessionTokenAsync())
                    return LastFmApiError.BadAuth;

            var resp = await _trackApi.UpdateNowPlayingAsync(new Scrobble(artist, album, name, played, duration, albumArtist));
            return resp.Error;
        }

        public async Task<LastFmApiError> ScrobbleAsync(string name, string artist, DateTime played, TimeSpan duration, string album = "",
            string albumArtist = "")
        {
            if (!_auth.Authenticated)
                if (!await GetSessionTokenAsync())
                    return LastFmApiError.BadAuth;
            
            var resp = await _trackApi.ScrobbleAsync(new Scrobble(artist, album, name, played, duration, albumArtist));
            return resp.Error;
        }

        public async Task<PageResponse<LastArtist>> GetRecommendedArtistsAsync(int page = 1, int limit = 30)
        {
            var resp = await _userApi.GetRecommendedArtistsAsync(page, limit);
            ThrowIfError(resp);
            return resp;
        }

        public async Task<LastAlbum> GetDetailAlbum(string name, string artist)
        {
            var resp = await _albumApi.GetAlbumInfoAsync(artist, name);
            ThrowIfError(resp);
            return resp.Content;
        }

        public async Task<LastAlbum> GetDetailAlbumByMbid(string mbid)
        {
            var resp = await _albumApi.GetAlbumInfoByMbidAsync(mbid);
            ThrowIfError(resp);
            return resp.Content;
        }

        public async Task<LastTrack> GetDetailTrack(string name, string artist)
        {
            var resp = await _trackApi.GetInfoAsync(name, artist);
            ThrowIfError(resp);
            return resp.Content;
        }

        public async Task<LastTrack> GetDetailTrackByMbid(string mbid)
        {
            var resp = await _trackApi.GetInfoByMbidAsync(mbid);
            ThrowIfError(resp);
            return resp.Content;
        }

        public async Task<LastArtist> GetDetailArtist(string name)
        {
            var resp = await _artistApi.GetArtistInfoAsync(name);
            ThrowIfError(resp);

            if (resp.Content != null && resp.Content.Bio != null)
            {
                //strip html tags
                var content = HtmlRemoval.StripTagsRegex(resp.Content.Bio.Content);

                try
                {
                    var startIndex = content.IndexOf("\n\n", StringComparison.Ordinal);
                    var endIndex = content.IndexOf("\n    \nUser-contributed", StringComparison.Ordinal);
                    var count = endIndex - startIndex;

                    //removing the read more on last.fm
                    content = content.Remove(startIndex, count);
                }
                catch
                {
                }

                //html decode
                resp.Content.Bio.Content = WebUtility.HtmlDecode(content);
            }
            return resp.Content;
        }

        public async Task<LastArtist> GetDetailArtistByMbid(string mbid)
        {
            var resp = await _artistApi.GetArtistInfoByMbidAsync(mbid);
            ThrowIfError(resp);
            return resp.Content;
        }

        public async Task<PageResponse<LastTrack>> GetArtistTopTracks(string name)
        {
            var resp = await _artistApi.GetTopTracksForArtistAsync(name);
            ThrowIfError(resp);
            return resp;
        }

        public async Task<PageResponse<LastAlbum>> GetArtistTopAlbums(string name)
        {
            var resp = await _artistApi.GetTopAlbumsForArtistAsync(name);
            ThrowIfError(resp);
            return resp;
        }

        public async Task<PageResponse<LastTrack>> SearchTracksAsync(string query, int page = 1, int limit = 30)
        {
            var resp = await _trackApi.SearchForTrackAsync(query, page, limit);
            ThrowIfError(resp);
            return resp;
        }

        public async Task<PageResponse<LastArtist>> SearchArtistsAsync(string query, int page = 1, int limit = 30)
        {
            var resp = await _artistApi.SearchForArtistAsync(query, page, limit);
            ThrowIfError(resp);
            return resp;
        }

        public async Task<PageResponse<LastAlbum>> SearchAlbumsAsync(string query, int page = 1, int limit = 30)
        {
            var resp = await _albumApi.SearchForAlbumAsync(query, page, limit);
            ThrowIfError(resp);
            return resp;
        }

        public async Task<PageResponse<LastTrack>> GetTopTracksAsync(int page = 1, int limit = 30)
        {
            var resp = await _chartApi.GetTopTracksAsync(page, limit);
            ThrowIfError(resp);
            return resp;
        }

        public async Task<PageResponse<LastArtist>> GetTopArtistsAsync(int page = 1, int limit = 30)
        {
            var resp = await _chartApi.GetTopArtistsAsync(page, limit);
            ThrowIfError(resp);
            return resp;
        }

        public async Task<List<LastArtist>> GetSimilarArtistsAsync(string name, int limit = 30)
        {
            var resp = await _artistApi.GetSimilarArtistsAsync(name, true, limit);
            ThrowIfError(resp);
            return resp.Content;
        }

        public async Task<List<LastTrack>> GetSimilarTracksAsync(string name, string artistName, int limit = 30)
        {
            var resp = await _trackApi.GetSimilarTracksAsync(name, artistName, true, limit);
            ThrowIfError(resp);
            return resp.Content;
        }

        public async Task<bool> AuthenticaAsync(string username, string password)
        {
            var result = await GetSessionTokenAsync(username, password);

            if (result)
            {
                CredentialHelper.SaveCredentials("lastfm", username, password);
            }

            return result;
        }


        private void ThrowIfError(LastResponse resp)
        {
            if (resp.Error != LastFmApiError.None)
                throw new LastException(resp.Error.ToString(), "API Error");
        }
    }
}