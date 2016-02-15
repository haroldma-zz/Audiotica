using Audiotica.Core.Common;

namespace Audiotica.Database.Models
{
    /// <summary>
    ///     Album object, not used in the database.
    /// </summary>
    public class Album : ObservableObject
    {
        private Artist _artist;

        public Album()
        {
            Tracks = new OptimizedObservableCollection<Track>();
        }

        public string Title { get; set; }
        public string Publisher { get; set; }
        public string Copyright { get; set; }
        public uint? Year { get; set; }
        public string ArtworkUri { get; set; }
        public OptimizedObservableCollection<Track> Tracks { get; set; }

        public Artist Artist
        {
            get { return _artist; }
            set { Set(ref _artist, value); }
        }
    }
}