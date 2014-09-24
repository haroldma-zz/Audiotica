#region

using System.Collections.Generic;

#endregion

namespace Audiotica.Data.Model.LastFm
{
    public class FmArtist
    {
        public string name { get; set; }
        public string listeners { get; set; }
        public string mbid { get; set; }
        public string url { get; set; }
        public List<FmImage> image { get; set; }
    }
}