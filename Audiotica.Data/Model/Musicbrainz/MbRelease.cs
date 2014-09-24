#region

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace Audiotica.Data.Model.Musicbrainz
{
    public class MbRelease
    {
        public string country { get; set; }
        public string status { get; set; }
        public string date { get; set; }
        public object barcode { get; set; }

        [JsonProperty(PropertyName = "release-events")]
        public List<ReleaseEvent> releaseEvents { get; set; }

        public string packaging { get; set; }
        public string disambiguation { get; set; }
        public string id { get; set; }
        public string title { get; set; }
        public object asin { get; set; }
        public string quality { get; set; }
    }

    public class ReleaseEvent
    {
        public string date { get; set; }
    }
}