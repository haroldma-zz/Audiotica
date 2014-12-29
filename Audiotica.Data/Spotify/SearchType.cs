using System;

namespace Audiotica.Data.Spotify
{
    [Flags]
    public enum SearchType
    {
        [String("artist")]
        ARTIST = 1,
        [String("album")]
        ALBUM = 2,
        [String("track")]
        TRACK = 4,
        [String("track,album,artist")]
        ALL = 8
    }
}
