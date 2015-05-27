using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Audiotica.Data.Spotify.Models
{
    public class FullArtist : SimpleArtist
    {
        [JsonProperty("genres")]
        public List<string> Genres { get; set; }
        [JsonProperty("images")]
        public List<Image> Images { get; set; }
        [JsonProperty("popularity")]
        public int Popularity { get; set; }
    }
}
