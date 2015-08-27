using Newtonsoft.Json;

namespace Audiotica.Web.Http.Requets.Metadata.LyricsFind.Models
{
    public class LyricsTrack
    {
        public string Lfid { get; set; }
        public int Amg { get; set; }
        public bool Instrumental { get; set; }
        public bool Viewable { get; set; }

        [JsonProperty("has_lrc")]
        public bool HasLrc { get; set; }

        [JsonProperty("lrc_verified")]
        public bool LrcVerified { get; set; }

        public string Title { get; set; }
        public LyricsFindArtist Artist { get; set; }

        [JsonProperty("last_update")]
        public string LastUpdate { get; set; }

        public string Lyrics { get; set; }
        public string Copyright { get; set; }
        public string Writer { get; set; }
    }
}