using System;

namespace Audiotica.Data.Spotify
{
    [Flags]
    public enum AlbumType
    {
        [String("album")]
        ALBUM = 1,
        [String("single")]
        SINGLE = 2,
        [String("compilation")]
        COMPILATION = 4,
        [String("appears_on")]
        APPEARS_ON = 8,
        [String("album,single,compilation,appears_on")]
        ALL = 16
    }
}
