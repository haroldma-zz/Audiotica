using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audiotica.Data.Model.LastFm
{
    public class FmDetailRoot
    {
        public FmDetailArtist artist { get; set; }
        public FmDetailAlbum album { get; set; }
        public FmDetailTrack track { get; set; }
    }
}
