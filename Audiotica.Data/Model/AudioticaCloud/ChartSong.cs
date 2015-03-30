using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audiotica.Data.Model.AudioticaCloud
{
    public class ChartSong
    {
        public enum Direction
        {
            None,
            Up,
            Down
        }

        public string Name { get; set; }
        public string ArtistName { get; set; }
        public string ArtistImage { get; set; }
        public string ArtistScreenName { get; set; }
        public List<double> Signals { get; set; }
        public Direction ChangeDirection { get; set; }
        public double SignalChangePercent { get; set; }
        public double PositionChangePercent { get; set; }
        public double PeakPosition { get; set; }
    }
}
