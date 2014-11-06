using Audiotica.Data.Collection.SqlHelper;

namespace Audiotica.Data.Collection.Model
{
    public class PlaylistSong : QueueSong
    {
        [SqlProperty(ReferenceTo = typeof(Playlist))]
        public long PlaylistId { get; set; }
    }
}