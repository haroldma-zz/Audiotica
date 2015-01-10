#region

using System.Threading.Tasks;
using Audiotica.Data.Model.Deezer;

#endregion

namespace Audiotica.Data.Service.Interfaces
{
    public interface IDeezerService
    {
        Task<DeezerPageResponse<DeezerSong>> GetArtistTopSongsAsync(int id, int page = 0, int limit = 10);
        Task<DeezerPageResponse<DeezerSong>> GetAlbumTrackListAsync(int id);

        Task<DeezerPageResponse<DeezerArtist>> SearchArtistsAsync(string query, int page = 0, int limit = 10);
        Task<DeezerPageResponse<DeezerSong>> SearchSongsAsync(string query, int page = 0, int limit = 10);
        Task<DeezerPageResponse<DeezerAlbum>> SearchAlbumsAsync(string query, int page = 0, int limit = 10);
    }
}