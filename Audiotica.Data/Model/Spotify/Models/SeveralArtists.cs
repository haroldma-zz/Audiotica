using System.Collections.Generic;
using Newtonsoft.Json;

namespace Audiotica.Data.Model.Spotify.Models
{
    public class SeveralArtists
    {
        [JsonProperty("artists")]
        public List<FullArtist> Artists { get; set; }
    }
}