using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audiotica.Data.Model.EchoNest
{
    public class EchoImage
    {
        public string url { get; set; }
        public List<object> tags { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public double aspect_ratio { get; set; }
        public bool verified { get; set; }
    }

    public class EchoArtistImagesRoot : EchoListResponse
    {
        public List<EchoImage> images { get; set; }
    }
}
