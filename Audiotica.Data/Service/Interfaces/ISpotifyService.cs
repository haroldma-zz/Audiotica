#region

using System.Collections.Generic;
using System.Threading.Tasks;
using Audiotica.Data.Model.Spotify.Models;

#endregion

namespace Audiotica.Data.Service.Interfaces
{
    public interface ISpotifyService
    {
        Task<FullArtist> GetArtistAsync(string id);
        Task<List<FullTrack>> GetArtistTracksAsync(string id);
        Task<Paging<SimpleAlbum>> GetArtistAlbumsAsync(string id);

        Task<FullAlbum> GetAlbumAsync(string id);
        Task<Paging<SimpleTrack>> GetAlbumTracksAsync(string id);
        Task<Paging<FullTrack>> SearchTracksAsync(string query, int limit = 20, int offset = 0);
        Task<Paging<SimpleArtist>> SearchArtistsAsync(string query, int limit = 20, int offset = 0);
        Task<Paging<SimpleAlbum>> SearchAlbumsAsync(string query, int limit = 20, int offset = 0);
    }
}