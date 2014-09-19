namespace Audiotica.Data.Collection.Model
{
    public class QueueSong
    {
        public long Id { get; set; }

        public Song Song { get; set; }

        public long SongId { get; set; }

        public long PrevId { get; set; }

        public long NextId { get; set; }
    }
}