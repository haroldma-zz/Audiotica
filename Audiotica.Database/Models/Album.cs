using System;
using System.Collections.Generic;

namespace Audiotica.Database.Models
{
    /// <summary>
    ///     Album object, not used in the database.
    /// </summary>
    public class Album
    {
        public Album()
        {
            Tracks = new List<Track>();
        }

        public string Title { get; set; }
        public string Publisher { get; set; }
        public string Copyright { get; set; }
        public int? Year { get; set; }
        public Uri ArtworkUri { get; set; }
        public List<Track> Tracks { get; set; }
        public Artist Artist { get; set; }
    }
}