using Newtonsoft.Json;

namespace Audiotica.Web.Models.Deezer
{
    public class DeezerSong
    {
        public int Id { get; set; }
        public bool Readable { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public int Duration { get; set; }
        public int Rank { get; set; }

        [JsonProperty("explicit_lyrics")]
        public bool ExplicitLyrics { get; set; }

        public string Preview { get; set; }
        public DeezerArtist Artist { get; set; }
        public DeezerAlbum Album { get; set; }
        public string Type { get; set; }
    }
}