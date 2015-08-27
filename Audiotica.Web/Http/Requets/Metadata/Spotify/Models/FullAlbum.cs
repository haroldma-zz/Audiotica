using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Audiotica.Web.Http.Requets.Metadata.Spotify.Models
{
    public class FullAlbum : SimpleAlbum
    {
        [JsonProperty("artists")]
        public List<SimpleArtist> Artists { get; set; }

        public SimpleArtist Artist => Artists.FirstOrDefault();

        [JsonProperty("genres")]
        public List<string> Genres { get; set; }

        [JsonProperty("popularity")]
        public int Popularity { get; set; }

        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }

        [JsonProperty("release_date_precision")]
        public string ReleaseDatePrecision { get; set; }

        [JsonProperty("tracks")]
        public Paging<SimpleTrack> Tracks { get; set; }
    }
}