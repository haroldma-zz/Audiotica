#region

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace Audiotica.Data.Model.Musicbrainz
{
    public class MbRelease
    {
       public string date { get; set; }

        [JsonProperty(PropertyName = "release-events")]
        public List<ReleaseEvent> releaseEvents { get; set; }

        public string id { get; set; }
        public string title { get; set; }
    }

    public class ReleaseEvent
    {
        public string date { get; set; }
    }
}