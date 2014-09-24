using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audiotica.Data.Model.LastFm
{
    public class FmDetailAlbum : FmAlbum
    {
        public string releasedate { get; set; }
        public string listeners { get; set; }
        public string playcount { get; set; }
        public FmTrackResults tracks { get; set; }
        public FmWiki wiki { get; set; }
    }

    public class FmWiki
    {
        public string published { get; set; }
        public string summary { get; set; }
        public string content { get; set; }
    }
}
