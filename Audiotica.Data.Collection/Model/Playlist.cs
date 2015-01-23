#region

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SQLite;

#endregion

namespace Audiotica.Data.Collection.Model
{
    public class Playlist : BaseEntry
    {
        internal readonly ConcurrentDictionary<long, PlaylistSong> LookupMap = new ConcurrentDictionary<long, PlaylistSong>();

        public Playlist()
        {
            Songs = new ObservableCollection<PlaylistSong>();
        }

        public string Name { get; set; }

        [Ignore]
        public ObservableCollection<PlaylistSong> Songs { get; set; }
    }
}