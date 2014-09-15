#region

using SQLite;

#endregion

namespace Audiotica.Collection.Model
{
    public class PlaylistSong : QueueSong
    {
        [Indexed]
        public int PlaylistId { get; set; }
    }
}