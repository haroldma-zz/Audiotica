using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Database.Models;

namespace Audiotica.Database.Services.Interfaces
{
    public interface ILibraryService
    {
        #region Properties

        /// <summary>
        ///     Gets the tracks.
        /// </summary>
        /// <value>
        ///     The tracks.
        /// </value>
        OptimizedObservableCollection<Track> Tracks { get; }

        /// <summary>
        ///     Gets the albums.
        /// </summary>
        /// <value>
        ///     The albums.
        /// </value>
        OptimizedObservableCollection<Album> Albums { get; }

        /// <summary>
        ///     Gets the artists.
        /// </summary>
        /// <value>
        ///     The artists.
        /// </value>
        OptimizedObservableCollection<Artist> Artists { get; }

        /// <summary>
        ///     Gets the playlists.
        /// </summary>
        /// <value>
        ///     The playlists.
        /// </value>
        OptimizedObservableCollection<Playlist> Playlists { get; }

        #endregion

        #region Helpers

        Track Find(long id);
        Track Find(Track track);
        Track Find(string title, string artists, string albumTitle, string albumArtist);

        #endregion

        #region Sync

        /// <summary>
        ///     Loads all the tracks from the database, then creates album and artist objects using their tags.
        /// </summary>
        void LoadLibrary();

        /// <summary>
        ///     Loads the playlists by reading the playlist files, serialized using json.
        /// </summary>
        void LoadPlaylists();

        /// <summary>
        ///     Adds the track to the library, saves it to the db.
        /// </summary>
        void AddTrack(Track track);

        #endregion

        #region Async

        /// <summary>
        ///     Loads all the tracks from the database, then creates album and artist objects using their tags.
        /// </summary>
        Task LoadLibraryAsync();

        /// <summary>
        ///     Loads the playlists by reading the playlist files, serialized using json.
        /// </summary>
        Task LoadPlaylistsAsync();

        /// <summary>
        ///     Adds the track to the library, saves it to the db.
        /// </summary>
        Task AddTrackAsync(Track track);

        #endregion
    }
}