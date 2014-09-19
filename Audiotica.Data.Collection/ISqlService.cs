#region

using System.Collections.Generic;
using System.Threading.Tasks;
using Audiotica.Data.Collection.Model;

#endregion

namespace Audiotica.Data.Collection
{
    //TODO [Harry,20140918] use reflection to make this more generic...
    public interface ISqlService
    {
        void Initialize();
        Task InitializeAsync();
        void ResetData();

        Task InsertSongAsync(Song song);
        Task InsertArtistAsync(Artist artist);
        Task InsertAlbumAsync(Album album);
        Task InsertQueueSongAsync(QueueSong song);
        Task InsertPlaylistSongAsync(PlaylistSong song);
        Task DeleteItemAsync(long id, string table);
        Task UpdateQueueSongAsync(QueueSong queue);

        Task<List<Song>>  GetSongsAsync();
        Task<List<Artist>>  GetArtistsAsync();
        Task<List<Album>>  GetAlbumsAsync();
        
        List<QueueSong> GetQueueSongs();
        Task<List<QueueSong>> GetQueueSongsAsync();

        Task DeleteTableAsync(string table);
    }
}