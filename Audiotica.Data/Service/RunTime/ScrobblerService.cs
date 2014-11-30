#region

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
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
        private static readonly LastAuth FmAuth = new LastAuth(ApiKeys.LastFmId, "");
        private readonly AlbumApi _albumApi = new AlbumApi(FmAuth);
        private readonly ArtistApi _artistApi = new ArtistApi(FmAuth);
        private readonly ChartApi _chartApi = new ChartApi(FmAuth);
        private readonly TrackApi _trackApi = new TrackApi(FmAuth);

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

        public async Task<PageResponse<LastArtist>> SearchArtistAsync(string query, int page = 1, int limit = 30)
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

        private void ThrowIfError(LastResponse resp)
        {
            if (resp.Error != LastFmApiError.None)
                throw new LastException(resp.Error.ToString(), "API Error");
        }
    }
}