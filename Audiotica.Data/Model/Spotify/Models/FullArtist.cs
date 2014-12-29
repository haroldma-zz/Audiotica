using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Audiotica.Data.Model.Spotify.Models
{
    public class FullArtist : SimpleArtist
    {
        [JsonProperty("genres")]
        public List<String> Genres { get; set; }
        [JsonProperty("images")]
        public List<Image> Images { get; set; }
        [JsonProperty("popularity")]
        public int Popularity { get; set; }
    }
}
