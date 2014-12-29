using Newtonsoft.Json;

namespace Audiotica.Data.Spotify.Models
{
    public class SearchItem : BasicModel
    {
        [JsonProperty("artists")]
        public Paging<FullArtist> Artists { get; set; }
        [JsonProperty("albums")]
        public Paging<SimpleAlbum> Albums { get; set; }
        [JsonProperty("tracks")]
        public Paging<FullTrack> Tracks { get; set; }
    }
}
