using Audiotica.Data.Collection.SqlHelper;

namespace Audiotica.Data.Collection.Model
{
    public class PlaylistSong : QueueSong
    {
        [SqlProperty(ReferenceTo = typeof(Playlist))]
        public long PlaylistId { get; set; }


        #region ignored from base

        [SqlIgnore]
        public new long ShuffleNextId { get; set; }

        [SqlIgnore]
        public new long ShufflePrevId { get; set; }

        #endregion
    }
}