using System;
using Newtonsoft.Json;

namespace Audiotica.Data.Spotify.Models
{
    public class PlaylistTrackCollection
    {
        [JsonProperty("href")]
        public String Href { get; set; }
        [JsonProperty("total")]
        public int Total { get; set; }
    }
}