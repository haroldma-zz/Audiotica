#region

using System.Collections.Generic;
using SQLite;

#endregion

namespace Audiotica.Collection.Model
{
    public class Playlist : BaseDbEntry
    {
        public string Name { get; set; }

        [Ignore]
        public List<PlaylistSong> Songs { get; set; }
    }
}