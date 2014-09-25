#region

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace Audiotica.Data.Model.LastFm
{
    public class FmSearchRoot
    {
        public FmResults results { get; set; }
    }

    public class FmResults
    {
        [JsonProperty(PropertyName = "opensearch:Query")]
        public OpensearchQuery Query { get; set; }

        [JsonProperty(PropertyName = "opensearch:totalResults")]
        public string totalResults { get; set; }

        [JsonProperty(PropertyName = "opensearch:startIndex")]
        public string startIndex { get; set; }

        [JsonProperty(PropertyName = "opensearch:itemsPerPage")]
        public string itemsPerPage { get; set; }

        public Trackmatches trackmatches { get; set; }

        public Artistmatches artistmatches { get; set; }

        public Albummatches albummatches { get; set; }
    }

    public class Trackmatches
    {
        public List<FmTrack> track { get; set; }
    }

    public class Artistmatches
    {
        public List<FmArtist> artist { get; set; }
    }

    public class Albummatches
    {
        public List<FmAlbum> album { get; set; }
    }

    public class OpensearchQuery
    {
        [JsonProperty(PropertyName = "#text")]
        public string text { get; set; }
        public string role { get; set; }
        public string searchTerms { get; set; }
        public string startPage { get; set; }
    }
}