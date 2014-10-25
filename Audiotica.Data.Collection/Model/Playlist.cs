#region

using System.Collections.ObjectModel;
using Audiotica.Data.Collection.SqlHelper;

#endregion

namespace Audiotica.Data.Collection.Model
{
    public class Playlist : BaseEntry
    {
        public string Name { get; set; }

        public ObservableCollection<PlaylistSong> Songs { get; set; }
    }
}