using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audiotica.Data.Model.EchoNest
{
    public class EchoArtistUrls
    {
        public string official_url { get; set; }
        public string lastfm_url { get; set; }
        public string twitter_url { get; set; }
        public string myspace_url { get; set; }
        public string wikipedia_url { get; set; }
        public string mb_url { get; set; }
    }

    public class EchoArtistUrlsRoot : EchoResponse
    {
        public EchoArtistUrls urls { get; set; }
    }
}
