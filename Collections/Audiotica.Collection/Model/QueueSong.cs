#region

using SQLite;

#endregion

namespace Audiotica.Collection.Model
{
    public class QueueSong : BaseDbEntry
    {
        [Ignore]
        public Song Song { get; set; }

        [Indexed]
        public int SongId { get; set; }

        [Indexed]
        public int PrevId { get; set; }

        [Indexed]
        public int NextId { get; set; }
    }
}