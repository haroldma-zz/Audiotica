#region

using System;
using Audiotica.Data.Collection.SqlHelper;
using SQLite;

#endregion

namespace Audiotica.Data.Collection.Model
{
    public class HistoryEntry : BaseEntry
    {
        [Indexed]
        public int SongId { get; set; }

        public bool CanScrobble { get; set; }
        public bool Scrobbled { get; set; }
        public DateTime DatePlayed { get; set; }

        [Ignore]
        public Song Song { get; set; }
    }
}