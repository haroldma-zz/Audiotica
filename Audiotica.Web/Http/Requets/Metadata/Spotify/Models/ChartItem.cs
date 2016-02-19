using Newtonsoft.Json;

namespace Audiotica.Web.Http.Requets.Metadata.Spotify.Models
{
    public class ChartItem
    {
        [JsonProperty("current_position")]
        public string CurrentPosition { get; set; }

        public string Plays { get; set; }

        [JsonProperty("previous_position")]
        public string PreviousPosition { get; set; }

        public FullTrack Track { get; set; }
    }
}