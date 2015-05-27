using Newtonsoft.Json;

namespace Audiotica.Web.Http.Requets.MatchEngine.Mp3Clan.Models
{
    public class Mp3ClanSong
    {
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Duration { get; set; }
        public string Url { get; set; }
        public string Genre { get; set; }
        public string Id { get; set; }

        [JsonProperty("lyrics_url")]
        public string LyricsUrl { get; set; }
    }
}