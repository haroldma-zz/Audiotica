using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;

namespace Audiotica.Database.Services.DesignTime
{
    public class DesignLibraryService : ILibraryService
    {
        public bool IsLoaded { get; }
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

        public void Load()
        {
            throw new System.NotImplementedException();
        }

        public Track AddTrack(Track track)
        {
            throw new System.NotImplementedException();
        }

        public Task LoadAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<Track> AddTrackAsync(Track track)
        {
            throw new System.NotImplementedException();
        }
    }
}