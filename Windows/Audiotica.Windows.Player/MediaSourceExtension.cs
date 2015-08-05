using Windows.Media.Core;
using Audiotica.Database.Models;

namespace Audiotica.Windows.Player
{
    internal static class MediaSourceExtension
    {
        private const string TrackKey = "track";

        private static object Get(MediaSource item, string key)
        {
            return item.CustomProperties[key];
        }

        private static void Set(MediaSource item, string key, object value)
        {
            item.CustomProperties[key] = value;
        }

        public static QueueTrack Queue(this MediaSource item)
        {
            return Get(item, TrackKey) as QueueTrack;
        }

        public static void Queue(this MediaSource item, QueueTrack queue)
        {
            Set(item, TrackKey, queue);
        }
    }
}