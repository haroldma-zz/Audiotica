using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Audiotica.Data.Model.Musicbrainz
{
    public class MbArtist
    {
        public string name { get; set; }

        [JsonProperty(PropertyName = "life-span")]
        public LifeSpan lifeSpan { get; set; }

        public string id { get; set; }
    }
    public class LifeSpan
    {
        public bool ended { get; set; }
        public string begin { get; set; }
        public string end { get; set; }
    }
}
