#region

using System.Threading.Tasks;
using Audiotica.Data.Model.Spotify.Models;
using IF.Lastfm.Core.Api.Helpers;

#endregion

namespace Audiotica.Data.Service.Interfaces
{
    public interface ISpotifyService
    {
        Task<Paging<FullTrack>> SearchTracksAsync(string query, int limit = 20, int offset = 0);
        Task<Paging<SimpleArtist>> SearchArtistsAsync(string query, int limit = 20, int offset = 0);
        Task<Paging<SimpleAlbum>> SearchAlbumsAsync(string query, int limit = 20, int offset = 0);
    }
}