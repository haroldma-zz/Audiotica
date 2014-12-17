using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audiotica.Data.Model
{
    public class Mp3ClanSong
    {
        public string artist { get; set; }
        public string title { get; set; }
        public string duration { get; set; }
        public string url { get; set; }
        public string genre { get; set; }
        public string tid { get; set; }
        public string lyrics_url { get; set; }
    }

    public class Mp3ClanRoot
    {
        public List<Mp3ClanSong> response { get; set; }
    }
}
