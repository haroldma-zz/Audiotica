using System.Collections.Generic;

namespace Audiotica.Web.Http.Requets.Metadata.Spotify.Models
{
    public class SpotifyChartsResponse
    {
        public List<ChartTrack> Tracks { get; set; }
        public string PrevDate { get; set; }
    }
}