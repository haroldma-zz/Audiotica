using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Database.Models;

namespace Audiotica.Database.Services.Interfaces
{
    public class DesignLibraryService : ILibraryService
    {
        public OptimizedObservableCollection<Track> Tracks { get; }
        public OptimizedObservableCollection<Album> Albums { get; }
        public OptimizedObservableCollection<Artist> Artists { get; }
        public OptimizedObservableCollection<Playlist> Playlists { get; }
        public Track Find(long id)
        {
            throw new System.NotImplementedException();
        }

        public Track Find(Track track)
        {
            throw new System.NotImplementedException();
        }

        public Track Find(string title, string artists, string albumTitle, string albumArtist)
        {
            throw new System.NotImplementedException();
        }

        public void LoadLibrary()
        {
            throw new System.NotImplementedException();
        }

        public void LoadPlaylists()
        {
            throw new System.NotImplementedException();
        }

        public void AddTrack(Track track)
        {
            throw new System.NotImplementedException();
        }

        public Task LoadLibraryAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task LoadPlaylistsAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task AddTrackAsync(Track track)
        {
            throw new System.NotImplementedException();
        }
    }
}