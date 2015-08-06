using SQLite;

namespace Audiotica.Database.Models
{
    public class PlaylistTrack : DatabaseEntryBase
    {
        public long PrevId { get; set; }
        public long TrackId { get; set; }
        public long NextId { get; set; }

        [Ignore]
        public Track Track { get; set; }
    }
}