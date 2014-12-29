#region

using System.Collections.Generic;
using System.Threading.Tasks;
using Audiotica.Data.Model.Spotify;
using Audiotica.Data.Model.Spotify.Models;
using Audiotica.Data.Service.Interfaces;

#endregion

namespace Audiotica.Data.Service.RunTime
{
    public class SpotifyService : ISpotifyService
    {
        private readonly SpotifyWebApi _spotify;

        public SpotifyService(SpotifyWebApi spotify)
        {
            _spotify = spotify;
        }

        public Task<FullArtist> GetArtistAsync(string id)
        {
            return _spotify.GetArtist(id);
        }

        public async Task<List<FullTrack>> GetArtistTracksAsync(string id)
        {
            return (await _spotify.GetArtistsTopTracks(id, "us")).Tracks;
        }

        public Task<Paging<SimpleAlbum>> GetArtistAlbumsAsync(string id)
        {
            return _spotify.GetArtistsAlbums(id, AlbumType.ALBUM, limit:50, market:"us");
        }

        public Task<FullAlbum> GetAlbumAsync(string id)
        {
            return _spotify.GetAlbum(id);
        }

        public Task<Paging<SimpleTrack>> GetAlbumTracksAsync(string id)
        {
            return _spotify.GetAlbumTracks(id);
        }

        public async Task<Paging<FullTrack>> SearchTracksAsync(string query, int limit = 20, int offset = 0)
        {
            var results = await _spotify.SearchItems(query, SearchType.TRACK, limit, offset);
            return results.Tracks;
        }

        public async Task<Paging<SimpleArtist>> SearchArtistsAsync(string query, int limit = 20, int offset = 0)
        {
            var results = await _spotify.SearchItems(query, SearchType.ARTIST, limit, offset);
            return results.Artists;
        }

        public async Task<Paging<SimpleAlbum>> SearchAlbumsAsync(string query, int limit = 20, int offset = 0)
        {
            var results = await _spotify.SearchItems(query, SearchType.ALBUM, limit, offset);
            return results.Albums;
        }
    }
}