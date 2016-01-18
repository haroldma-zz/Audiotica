using System.Collections.Generic;

namespace Audiotica.Web.Http.Requets.Metadata.Spotify.Models
{
    public class SpotifyChartsResponse
    {
        public string Country { get; set; }

        public string Date { get; set; }

        public string Description { get; set; }

        public ChartEntry Entries { get; set; }

        public string Href { get; set; }

        public List<Image> Images { get; set; }

        public string Name { get; set; }

        public string Recurrence { get; set; }

        public string Type { get; set; }
    }
}