using Audiotica.Data.Collection.SqlHelper;

namespace Audiotica.Data.Collection.Model
{
    public class QueueSong : BaseEntry
    {
        public Song Song { get; set; }

        [SqlProperty(ReferenceTo = typeof(Song))]
        public long SongId { get; set; }

        public long PrevId { get; set; }

        public long NextId { get; set; }
    }
}