#region

using System.Collections.Generic;
using System.Threading.Tasks;
using Audiotica.Data.Spotify.Models;

#endregion

namespace Audiotica.Data.Service.Interfaces
{
    public interface ISpotifyService
    {
        Task<List<ChartTrack>> GetViralTracksAsync(string market = "us", string time = "weekly");
        Task<List<ChartTrack>> GetMostStreamedTracksAsync(string market = "us", string time = "weekly");

        Task<FullArtist> GetArtistAsync(string id);
        Task<List<FullTrack>> GetArtistTracksAsync(string id);
        Task<Paging<SimpleAlbum>> GetArtistAlbumsAsync(string id);

        Task<FullAlbum> GetAlbumAsync(string id);
        Task<Paging<SimpleTrack>> GetAlbumTracksAsync(string id);
        Task<Paging<FullTrack>> SearchTracksAsync(string query, int limit = 20, int offset = 0);
        Task<Paging<FullArtist>> SearchArtistsAsync(string query, int limit = 20, int offset = 0);
        Task<Paging<SimpleAlbum>> SearchAlbumsAsync(string query, int limit = 20, int offset = 0);
    }
}