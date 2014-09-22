#region

using System.Collections.Generic;
using Audiotica.Data.Collection.SqlHelper;

#endregion

namespace Audiotica.Data.Collection.Model
{
    public class Playlist : BaseEntry
    {
        public string Name { get; set; }

        [SqlIgnore]
        public List<PlaylistSong> Songs { get; set; }
    }
}