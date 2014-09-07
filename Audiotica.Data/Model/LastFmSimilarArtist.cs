using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Audiotica.Data.Model
{
    public class LastFmArtist
    {
        public string name { get; set; }
        public string mbid { get; set; }
        public string match { get; set; }
        public string url { get; set; }
        public List<LastFmImage> image { get; set; }

        public string LargeImage
        {
            get { return image.FirstOrDefault(p => p.size == "large").url; }
        }
    }

    public class LastFmImage
    {
        [JsonProperty(PropertyName = "#text")]
        public string url { get; set; }
        public string size { get; set; }
    }

    public class Similarartists
    {
        public List<LastFmArtist> artist { get; set; }
    }

    public class LastFmSimilarRoot
    {
        public Similarartists similarartists { get; set; }
    }
}
