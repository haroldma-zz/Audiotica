using System;
using System.Collections.Generic;

namespace Audiotica.Database.Models
{
    /// <summary>
    ///     Artist object, not used in the database.
    /// </summary>
    public class Artist
    {
        public string Name { get; set; }
        public List<Track> Tracks { get; set; }
        public List<Track> TracksThatAppearsIn { get; set; }
        public List<Album> Albums { get; set; }
        public Uri ArtworkPath { get; set; }
    }
}