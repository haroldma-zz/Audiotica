#region

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

        public async Task<FullAlbum> GetAlbumAsync(string id)
        {
            var result = await _spotify.GetAlbum(id);
            return result;
        }

        public async Task<Paging<SimpleTrack>> GetAlbumTracksAsync(string id)
        {
            var result = await _spotify.GetAlbumTracks(id);
            return result;
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