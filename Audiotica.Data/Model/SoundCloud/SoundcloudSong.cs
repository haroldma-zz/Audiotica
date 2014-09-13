namespace Audiotica.Data.Model.SoundCloud
{
    public class SoundCloudSong
    {
        public int id { get; set; }
        public string created_at { get; set; }
        public SoundCloudUser user { get; set; }

        public int duration { get; set; }
        public bool streamable { get; set; }

        public string track_type { get; set; }

        public string genre { get; set; }
        public string title { get; set; }

        public string artwork_url { get; set; }

        public string stream_url { get; set; }

        public int playback_count { get; set; }
        public int favoritings_count { get; set; }
        public int likes_count { get; set; }

    }
}
