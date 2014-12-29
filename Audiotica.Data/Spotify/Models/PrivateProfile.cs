using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Audiotica.Data.Spotify.Models
{
    public class PrivateProfile : BasicModel
    {
        [JsonProperty("country")]
        public String Country { get; set; }
        [JsonProperty("display_name")]
        public String DisplayName { get; set; }
        [JsonProperty("external_urls")]
        public Dictionary<string, string> ExternalUrls { get; set; }
        [JsonProperty("href")]
        public String Href { get; set; }
        [JsonProperty("id")]
        public String Id { get; set; }
        [JsonProperty("images")]
        public List<Image> Images { get; set; }
        [JsonProperty("product")]
        public String Product { get; set; }
        [JsonProperty("type")]
        public String Type { get; set; }
        [JsonProperty("uri")]
        public String Uri { get; set; }
    }
}
