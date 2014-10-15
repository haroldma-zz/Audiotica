#region

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Audiotica.Data.Collection.Model;

#endregion

namespace Audiotica.Data.Collection
{
    public interface ICollectionService
    {
        ObservableCollection<Song> Songs { get; set; }
        ObservableCollection<Album> Albums { get; set; }
        ObservableCollection<Artist> Artists { get; set; }

        /// <summary>
        ///     Loads all songs, albums, artist and playlists/queue.
        /// </summary>
        void LoadLibrary();

        Task LoadLibraryAsync();

        /// <summary>
        ///     Adds the song to the database and collection.
        /// </summary>
        Task AddSongAsync(Song song, string artworkUrl);

        /// <summary>
        ///     Deletes the song from the database and collection.  Also all related files.
        /// </summary>
        Task DeleteSongAsync(Song song);

        //TODO [Harry,20140915] everything related to the queue and playlists
    }
}