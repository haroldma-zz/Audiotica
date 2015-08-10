using Newtonsoft.Json;

namespace Audiotica.Web.Http.Requets.Metadata.Spotify.Models
{
    public class ChartTrack
    {
        public string Date { get; set; }
        public string Country { get; set; }

        [JsonProperty("track_id")]
        public string Id => Url.Replace("https://play.spotify.com/track/", "");

        [JsonProperty("artist_id")]
        public string ArtistId => ArtistUrl.Replace("https://play.spotify.com/artist/", "");

        [JsonProperty("album_id")]
        public string AlbumId => AlbumUrl.Replace("https://play.spotify.com/album/", "");

        [JsonProperty("track_url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "track_name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "artist_name")]
        public string ArtistName { get; set; }

        [JsonProperty("artist_url")]
        public string ArtistUrl { get; set; }

        [JsonProperty("album_name")]
        public string AlbumName { get; set; }

        [JsonProperty("album_url")]
        public string AlbumUrl { get; set; }

        [JsonProperty(PropertyName = "artwork_url")]
        public string ArtworkUrl { get; set; }

        [JsonProperty("num_streams")]
        public int NumStreams { get; set; }
    }
}