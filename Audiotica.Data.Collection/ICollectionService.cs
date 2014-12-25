#region

using System.Collections.Generic;
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
        ObservableCollection<Playlist> Playlists { get; set; }
        ObservableCollection<QueueSong> PlaybackQueue { get; }

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

        Task<List<HistoryEntry>> FetchHistoryAsync(); 

        #region Playback Queue

        /// <summary>
        /// Erases all songs in the playback queue (database and  PlaybackQueue prop)
        /// </summary>
        /// <returns></returns>
        Task ClearQueueAsync();

        /// <summary>
        /// Adds the current song to the end of the queue.
        /// </summary>
        Task AddToQueueAsync(Song song);

        /// <summary>
        /// Moves the queue items at the old index to the new index
        /// </summary>
        Task MoveQueueFromToAsync(int oldIndex, int newIndex);

        /// <summary>
        /// Deletes the specify song from the queue (database and PlaybackQueue prop)
        /// </summary>
        /// <param name="songToRemove"></param>
        /// <returns></returns>
        Task DeleteFromQueueAsync(Song songToRemove);

        #endregion

        #region Playlist

        Task<Playlist> CreatePlaylistAsync(string name);

        Task DeletePlaylistAsync(Playlist playlist);

        Task AddToPlaylistAsync(Playlist playlist, Song song);

        Task MovePlaylistFromToAsync(Playlist playlist, int oldIndex, int newIndex);

        Task DeleteFromPlaylistAsync(Playlist playlist, PlaylistSong songToRemove);

        #endregion
    }
}