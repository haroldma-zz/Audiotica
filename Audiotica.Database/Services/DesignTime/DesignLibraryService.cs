using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;

namespace Audiotica.Database.Services.DesignTime
{
    public class DesignLibraryService : ILibraryService
    {
        public bool IsLoaded { get; }
        public OptimizedObservableCollection<Track> Tracks { get; private set; }
        public OptimizedObservableCollection<Album> Albums { get; private set; }
        public OptimizedObservableCollection<Artist> Artists { get; private set; }
        public OptimizedObservableCollection<Playlist> Playlists { get; private set; }
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
            const string kauaiArtwork = "https://runthetrap.com/wp-content/uploads/2014/10/stream-childish-gambino-kauai-ep1.jpg";
            const string gambinoArtwork = "http://static1.1.sqspcdn.com/static/f/362468/13350786/1311549110307/Childish-Gambino.jpg?token=%2F%2FxvckXhTw1vqbFHcrSvgEN5hhE%3D";

            Tracks = new OptimizedObservableCollection<Track>
            {
                new Track
                {
                    Title = "Sober",
                    DisplayArtist = "Childish Gambino",
                    AlbumTitle = "Kauai",
                    ArtworkUri = gambinoArtwork,
                    ArtistArtworkUri = kauaiArtwork
                }
            };
        }

        public void AddTrack(Track track)
        {
            throw new System.NotImplementedException();
        }

        public void UpdateTrack(Track track)
        {
            throw new System.NotImplementedException();
        }

        public Task LoadAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task AddTrackAsync(Track track)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateTrackAsync(Track track)
        {
            throw new System.NotImplementedException();
        }
    }
}