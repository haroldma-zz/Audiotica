using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Audiotica.Data.Model.LastFm
{ 
    public class FmDetailTrack : FmTrack
    {
        public string id { get; set; }
        public string playcount { get; set; }
        public new FmArtist artist { get; set; }
        public FmAlbum album { get; set; }
    }
}
