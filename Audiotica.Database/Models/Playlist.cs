using System;
using System.Collections.Generic;

namespace Audiotica.Database.Models
{
    /// <summary>
    /// Playlist objects, not used in the database.
    /// </summary>
    public class Playlist
    {
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<Track> Tracks { get; set; }
    }
}