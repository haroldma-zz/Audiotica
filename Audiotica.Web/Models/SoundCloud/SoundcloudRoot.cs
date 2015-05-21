using System.Collections.Generic;
using Newtonsoft.Json;

namespace Audiotica.Web.Models.SoundCloud
{
    public class SoundCloudRoot
    {
        public List<SoundCloudSong> Collection { get; set; }

        [JsonProperty("next_href")]
        public string NextHref { get; set; }
    }
}