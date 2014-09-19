#region

using System.Collections.Generic;

#endregion

namespace Audiotica.Data.Collection.Model
{
    public class Playlist
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<PlaylistSong> Songs { get; set; }
    }
}