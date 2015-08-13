using System;
using System.Collections.Generic;

namespace Audiotica.Database.Models
{
    /// <summary>
    ///     Playlist objects, not used in the database.
    /// </summary>
    public class Playlist
    {
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Name { get; set; }
        public List<PlaylistTrack> Tracks { get; set; }
    }
}