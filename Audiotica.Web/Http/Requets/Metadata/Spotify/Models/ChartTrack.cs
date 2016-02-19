using Newtonsoft.Json;
using System.Collections.Generic;

namespace Audiotica.Web.Http.Requets.Metadata.Spotify.Models
{
    public class ChartEntry
    {
        public string Href { get; set; }

        public List<ChartItem> Items { get; set; }

        public string Limit { get; set; }

        public string Next { get; set; }

        public string Offset { get; set; }

        public string Previous { get; set; }

        public string Total { get; set; }
    }
}