#region

using System;
using Audiotica.Data.Collection.SqlHelper;

#endregion

namespace Audiotica.Data.Collection.Model
{
    public class HistoryEntry : BaseEntry
    {
        public long SongId { get; set; }

        public bool CanScrobble { get; set; }
        public bool Scrobbled { get; set; }
        public DateTime DatePlayed { get; set; }
        public DateTime DateEnded { get; set; }

        public Song Song { get; set; }
    }
}