#region

using System.Collections.Generic;

#endregion

namespace Audiotica.Data.Model.LastFm
{
    public class FmTrack
    {
        public string name { get; set; }
        public string artist { get; set; }
        public string url { get; set; }
        public string listeners { get; set; }
        public List<FmImage> image { get; set; }
        public string mbid { get; set; }
    }
}