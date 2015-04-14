using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Utils;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Data.Service.Interfaces;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;

namespace Audiotica.Data.Service.RunTime
{
    public class ScrobblerService : IScrobblerService
    {
        private readonly AlbumApi _albumApi;
        private readonly ArtistApi _artistApi;
        private readonly ChartApi _chartApi;
        private readonly ICredentialHelper _credentialHelper;
        private readonly TrackApi _trackApi;
        private readonly UserApi _userApi;
        private LastAuth _auth;

        public ScrobblerService(ICredentialHelper credentialHelper)
        {
            _credentialHelper = credentialHelper;
            _auth = new LastAuth(ApiKeys.LastFmId, ApiKeys.LastFmSecret);
            _albumApi = new AlbumApi(_auth);
            _artistApi = new ArtistApi(_auth);
            _chartApi = new ChartApi(_auth);
            _trackApi = new TrackApi(_auth);
            _userApi = new UserApi(_auth);
            GetSessionTokenAsync();
        }

        public event EventHandler<BoolEventArgs> AuthStateChanged;

        public bool HasCredentials
        {
            get { return _credentialHelper.GetCredentials("lastfm") != null; }
        }

        public bool IsAuthenticated
        {
            get { return _auth.Authenticated; }
        }

        public void Logout()
        {
            _credentialHelper.DeleteCredentials("lastfm");
            _auth = new LastAuth(ApiKeys.LastFmId, ApiKeys.LastFmSecret);
            OnAuthStateChanged();
        }

        public async Task<LastResponseStatus> ScrobbleNowPlayingAsync(string name, string artist, DateTime played,
            TimeSpan? duration, string album = "",
            string albumArtist = "")
        {
            if (!_auth.Authenticated)
                if (!await GetSessionTokenAsync())
                    return LastResponseStatus.BadAuth;

            var resp = await _trackApi.UpdateNowPlayingAsync(new Scrobble(artist, album, name, played)
            {
                Duration = duration,
                AlbumArtist = albumArtist
            });
            return resp.Status;
        }

        public async Task<LastResponseStatus> ScrobbleAsync(string name, string artist, DateTime played,
            TimeSpan? duration,
            string album = "",
            string albumArtist = "")
        {
            if (!_auth.Authenticated)
                if (!await GetSessionTokenAsync())
                    return LastResponseStatus.BadAuth;
            
            var resp = await _trackApi.ScrobbleAsync(new Scrobble(artist, album, name, played)
            {
                Duration = duration,
                AlbumArtist = albumArtist
            });
            return resp.Status;
        }

        public async Task<PageResponse<LastArtist>> GetRecommendedArtistsAsync(int page = 1, int limit = 30)
        {
            var resp = await _userApi.GetRecommendedArtistsAsync(page, limit);
            return resp;
        }

        public async Task<LastAlbum> GetDetailAlbum(string name, string artist)
        {
            var resp = await _albumApi.GetAlbumInfoAsync(artist, name);
            return resp.Success ? resp.Content : null;
        }

        public async Task<LastAlbum> GetDetailAlbumByMbid(string mbid)
        {
            var resp = await _albumApi.GetAlbumInfoByMbidAsync(mbid);
            return resp.Success ? resp.Content : null;
        }

        public async Task<LastTrack> GetDetailTrack(string name, string artist)
        {
            var resp = await _trackApi.GetInfoAsync(name, artist);
            return resp.Success ? resp.Content : null;
        }

        public async Task<LastTrack> GetDetailTrackByMbid(string mbid)
        {
            var resp = await _trackApi.GetInfoByMbidAsync(mbid);
            return resp.Success ? resp.Content : null;
        }

        public async Task<LastArtist> GetDetailArtist(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            var resp = await _artistApi.GetArtistInfoAsync(name);

            if (!resp.Success) return null;

            if (resp.Content != null && resp.Content.Bio != null)
            {
                //strip html tags
                var content = resp.Content.Bio.Content.StripHtmlTags();

                if (!string.IsNullOrEmpty(content))
                {
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
            }
            return resp.Content;
        }

        public async Task<LastArtist> GetDetailArtistByMbid(string mbid)
        {
            var resp = await _artistApi.GetArtistInfoByMbidAsync(mbid);
            return resp.Content;
        }

        public async Task<PageResponse<LastTrack>> GetArtistTopTracks(string name)
        {
            var resp = await _artistApi.GetTopTracksForArtistAsync(name);
            return resp;
        }

        public async Task<PageResponse<LastAlbum>> GetArtistTopAlbums(string name)
        {
            var resp = await _artistApi.GetTopAlbumsForArtistAsync(name);
            return resp;
        }

        public async Task<PageResponse<LastTrack>> SearchTracksAsync(string query, int page = 1, int limit = 30)
        {
            var resp = await _trackApi.SearchForTrackAsync(query, page, limit);
            return resp;
        }

        public async Task<PageResponse<LastArtist>> SearchArtistsAsync(string query, int page = 1, int limit = 30)
        {
            var resp = await _artistApi.SearchForArtistAsync(query, page, limit);
            return resp;
        }

        public async Task<PageResponse<LastAlbum>> SearchAlbumsAsync(string query, int page = 1, int limit = 30)
        {
            var resp = await _albumApi.SearchForAlbumAsync(query, page, limit);
            return resp;
        }

        public async Task<PageResponse<LastTrack>> GetTopTracksAsync(int page = 1, int limit = 30)
        {
            var resp = await _chartApi.GetTopTracksAsync(page, limit);
            return resp;
        }

        public async Task<PageResponse<LastArtist>> GetTopArtistsAsync(int page = 1, int limit = 30)
        {
            var resp = await _chartApi.GetTopArtistsAsync(page, limit);
            return resp;
        }

        public async Task<List<LastArtist>> GetSimilarArtistsAsync(string name, int limit = 30)
        {
            var resp = await _artistApi.GetSimilarArtistsAsync(name, true, limit);
            return resp.Success ? resp.Content.ToList() : null;
        }

        public async Task<List<LastTrack>> GetSimilarTracksAsync(string name, string artistName, int limit = 30)
        {
            var resp = await _trackApi.GetSimilarTracksAsync(name, artistName, true, limit);
            return resp.Success ? resp.Content.ToList() : null;
        }

        public async Task<bool> AuthenticaAsync(string username, string password)
        {
            var result = await GetSessionTokenAsync(username, password);

            if (result)
            {
                _credentialHelper.SaveCredentials("lastfm", username, password);
            }

            return result;
        }

        protected virtual void OnAuthStateChanged()
        {
            var handler = AuthStateChanged;
            if (handler != null) handler(this, new BoolEventArgs(IsAuthenticated));
        }

        private async Task<bool> GetSessionTokenAsync()
        {
            var creds = _credentialHelper.GetCredentials("lastfm");

            if (creds == null) return false;

            var result = await GetSessionTokenWithResultsAsync(creds.GetUsername(), creds.GetPassword());

            if (result == LastResponseStatus.BadAuth)
            {
                Logout();
            }

            OnAuthStateChanged();

            return result == LastResponseStatus.Successful;
        }

        private async Task<bool> GetSessionTokenAsync(string username, string password)
        {
            var response = await GetSessionTokenWithResultsAsync(username, password);
            OnAuthStateChanged();
            return response == LastResponseStatus.Successful;
        }

        private async Task<LastResponseStatus> GetSessionTokenWithResultsAsync(string username, string password)
        {
            try
            {
                var response = await _auth.GetSessionTokenAsync(username, password);
                return response.Status;
            }
            catch
            {
                return LastResponseStatus.RequestFailed;
            }
        }
    }
}