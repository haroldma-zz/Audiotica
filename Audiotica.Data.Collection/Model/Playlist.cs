#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Audiotica.Data.Collection.SqlHelper;

#endregion

namespace Audiotica.Data.Collection.Model
{
    public class Playlist : BaseEntry
    {
        internal readonly Dictionary<long, PlaylistSong> LookupMap = new Dictionary<long, PlaylistSong>();

        public Playlist()
        {
            Songs = new ObservableCollection<PlaylistSong>();
        }

        public string Name { get; set; }

        public ObservableCollection<PlaylistSong> Songs { get; set; }
    }
}