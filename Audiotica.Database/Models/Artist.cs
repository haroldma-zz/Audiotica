using Audiotica.Core.Common;

namespace Audiotica.Database.Models
{
    /// <summary>
    ///     Artist object, not used in the database.
    /// </summary>
    public class Artist
    {
        public Artist()
        {
            Albums = new OptimizedObservableCollection<Album>();
            Tracks = new OptimizedObservableCollection<Track>();
            TracksThatAppearsIn = new OptimizedObservableCollection<Track>();
        }

        public string Name { get; set; }
        public OptimizedObservableCollection<Track> Tracks { get; set; }
        public OptimizedObservableCollection<Track> TracksThatAppearsIn { get; set; }
        public OptimizedObservableCollection<Album> Albums { get; set; }
        public string ArtworkUri { get; set; }
    }
}