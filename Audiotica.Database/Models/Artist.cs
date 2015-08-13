using System;
using System.Collections.Generic;

namespace Audiotica.Database.Models
{
    /// <summary>
    ///     Artist object, not used in the database.
    /// </summary>
    public class Artist
    {
        public Artist()
        {
            Albums = new List<Album>();
            Tracks = new List<Track>();
            TracksThatAppearsIn = new List<Track>();
        }

        public string Name { get; set; }
        public List<Track> Tracks { get; set; }
        public List<Track> TracksThatAppearsIn { get; set; }
        public List<Album> Albums { get; set; }
        public string ArtworkUri { get; set; }
    }
}