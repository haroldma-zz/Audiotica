using SQLite;

namespace Audiotica.Data.Collection.Model
{
    public class PlaylistSong : QueueSong
    {
        public long PlaylistId { get; set; }

        #region ignored from base

        [Ignore]
        public new long ShuffleNextId { get; set; }

        [Ignore]
        public new long ShufflePrevId { get; set; }

        #endregion
    }
}