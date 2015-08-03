using Windows.Media.Core;

namespace Audiotica.Windows.Player
{
    internal static class PlaybackItem
    {
        private const string TrackIdKey = "trackid";
        private const string TitleKey = "title";
        private const string ArtistKey = "artist";
        private const string AlbumKey = "album";
        private const string AlbumArtistKey = "albumartist";
        private const string AlbumArtKey = "albumart";

        private static object Get(MediaSource item, string key)
        {
            return item.CustomProperties[key];
        }

        private static void Set(MediaSource item, string key, object value)
        {
            item.CustomProperties[key] = value;
        }

        public static int Id(this MediaSource item)
        {
            return Get(item, TrackIdKey) as int? ?? -1;
        }

        public static void Id(this MediaSource item, int id)
        {
            Set(item, TrackIdKey, id);
        }

        public static string Title(this MediaSource item)
        {
            return Get(item, TitleKey) as string;
        }

        public static void Title(this MediaSource item, string title)
        {
            Set(item, TitleKey, title);
        }

        public static string Artists(this MediaSource item)
        {
            return Get(item, ArtistKey) as string;
        }

        public static void Artists(this MediaSource item, string artist)
        {
            Set(item, ArtistKey, artist);
        }

        public static string AlbumTitle(this MediaSource item)
        {
            return Get(item, AlbumKey) as string;
        }

        public static void AlbumTitle(this MediaSource item, string album)
        {
            Set(item, AlbumKey, album);
        }

        public static string AlbumArtist(this MediaSource item)
        {
            return Get(item, AlbumArtistKey) as string;
        }

        public static void AlbumArtist(this MediaSource item, string albumArtist)
        {
            Set(item, AlbumArtistKey, albumArtist);
        }

        public static string Artwork(this MediaSource item)
        {
            return Get(item, AlbumArtKey) as string;
        }

        public static void Artwork(this MediaSource item, string artwork)
        {
            Set(item, AlbumArtKey, artwork);
        }
    }
}