using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audiotica.Data.Model
{
    public class MeileSong
    {
        public string name { get; set; }
        public int id { get; set; }
        public int duration { get; set; }
        public string artistName { get; set; }
        public string mp3 { get; set; }
        public string normalCover { get; set; }
    }

    public class Values
    {
        public List<MeileSong> songs { get; set; }
    }

    public class MeileDetailRoot
    {
        public Values values { get; set; }
        public int code { get; set; }
    }
}
