using System;
using System.Collections.Generic;
using SQLite;

namespace Audiotica.Database.Models
{
    /// <summary>
    /// Playlist objects, not used in the database.
    /// </summary>
    public class Playlist : DatabaseEntryBase
    {
        public string Name { get; set; }
        [Ignore]
        public List<PlaylistTrack> Tracks { get; set; }
    }
}