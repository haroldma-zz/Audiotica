#region

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Audiotica.Core.Exceptions;
using Audiotica.Core.Utilities;
using Audiotica.Data.Model.Musicbrainz;
using Audiotica.Data.Service.Interfaces;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.Data.Service.RunTime
{
    public class ScrobblerService : IScrobblerService
    {
        private const string MbApiPath =
            "http://musicbrainz.org/ws/2/{0}/{1}?fmt=json";

        private static readonly Auth FmAuth = new Auth(ApiKeys.LastFmId, "");
        private readonly AlbumApi _albumApi = new AlbumApi(FmAuth);
        private readonly ArtistApi _artistApi = new ArtistApi(FmAuth);
        private readonly TrackApi _trackApi = new TrackApi(FmAuth);
        private readonly ChartApi _chartApi = new ChartApi(FmAuth);

        public async Task<MbRelease> GetMbAlbum(string id)
        {
            var url = string.Format(MbApiPath, "release", id);
            var resp = await GetAsync<MbRelease>(url);
            return resp;
        }

        public async Task<MbArtist> GetMbArtist(string id)
        {
            var url = string.Format(MbApiPath, "artist", id);
            var resp = await GetAsync<MbArtist>(url);
            return resp;
        }

        public async Task<LastAlbum> GetDetailAlbum(string name, string artist)
        {
            var resp = await _albumApi.GetAlbumInfoAsync(artist, name);
            return resp.Content;
        }

        public async Task<LastAlbum> GetDetailAlbumByMbid(string mbid)
        {
            var resp = await _albumApi.GetAlbumInfoByMbidAsync(mbid);
            return resp.Content;
        }

        public async Task<LastTrack> GetDetailTrack(string name, string artist)
        {
            var resp = await _trackApi.GetInfoAsync(name, artist);
            return resp.Content;
        }

        public async Task<LastTrack> GetDetailTrackByMbid(string mbid)
        {
            var resp = await _trackApi.GetInfoByMbidAsync(mbid);
            return resp.Content;
        }

        public async Task<LastArtist> GetDetailArtist(string name)
        {
            var resp = await _artistApi.GetArtistInfoAsync(name);
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

        public async Task<PageResponse<LastArtist>> SearchArtistAsync(string query, int page = 1, int limit = 30)
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
            return resp.Content;
        }

        public Task<List<LastTrack>> GetSimilarTracksAsync(string name, string artistName, int limit = 30)
        {
            throw new System.NotImplementedException();
        }

        private void ThrowIfError(HttpResponseMessage resp)
        {
            if (!resp.IsSuccessStatusCode)
                throw new NetworkException();
        }

        private async Task<T> GetAsync<T>(string url)
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(url);
                ThrowIfError(resp);
                var json = await resp.Content.ReadAsStringAsync();
                var parseResp = await json.DeserializeAsync<T>();

                return parseResp;
            }
        }
    }
}