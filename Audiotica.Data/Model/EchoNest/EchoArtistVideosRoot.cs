using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audiotica.Data.Model.EchoNest
{
    public class EchoVideo
    {
        public string title { get; set; }
        public string url { get; set; }
        public string site { get; set; }
        public string date_found { get; set; }
        public string image_url { get; set; }
        public string id { get; set; }
    }

    public class EchoArtistVideosRoot : EchoListResponse
    {
        public List<EchoVideo> video { get; set; }
    }
}
