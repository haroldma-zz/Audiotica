using SQLite;

namespace Audiotica.Data.Collection.Model
{
    public class QueueSong : BaseEntry
    {
        [Ignore]
        public Song Song { get; set; }

        [Indexed]
        public int SongId { get; set; }

        [Indexed]
        public int PrevId { get; set; }

        [Indexed]
        public int NextId { get; set; }

        [Indexed]
        public int ShuffleNextId { get; set; }

        [Indexed]
        public int ShufflePrevId { get; set; }
    }
}