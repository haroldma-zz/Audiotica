using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audiotica.Data.Model.LastFm
{
    public class FmDetailArtist : FmArtist
    {
        public bool ontour { get; set; }
        public Stats stats { get; set; }
        public FmArtistResults similar { get; set; }
        public Bio bio { get; set; }
    }

    public class Bio
    {
        public string published { get; set; }
        public string summary { get; set; }
        public string content { get; set; }
        public string yearformed { get; set; }
        public Formationlist formationlist { get; set; }
    }

    public class Formation
    {
        public string yearfrom { get; set; }
        public string yearto { get; set; }
    }

    public class Formationlist
    {
        public Formation formation { get; set; }
    }

    public class Stats
    {
        public string listeners { get; set; }
        public string playcount { get; set; }
    }
}
