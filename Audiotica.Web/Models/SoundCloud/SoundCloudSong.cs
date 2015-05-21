using Newtonsoft.Json;

namespace Audiotica.Web.Models.SoundCloud
{
    public class SoundCloudSong
    {
        public int Id { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        public SoundCloudUser User { get; set; }
        public int Duration { get; set; }
        public bool Streamable { get; set; }

        [JsonProperty("track_type")]
        public string TrackType { get; set; }

        public string Genre { get; set; }
        public string Title { get; set; }

        [JsonProperty("artwork_url")]
        public string ArtworkUrl { get; set; }

        [JsonProperty("stream_url")]
        public string StreamUrl { get; set; }

        [JsonProperty("playback_count")]
        public int PlaybackCount { get; set; }

        [JsonProperty("favoritings_count")]
        public int FavoritingsCount { get; set; }

        [JsonProperty("likes_count")]
        public int LikesCount { get; set; }

        [JsonProperty("original_content_size")]
        public int OriginalContentSize { get; set; }
    }
}