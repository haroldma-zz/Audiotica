using System.Collections.Generic;
using Newtonsoft.Json;

namespace Audiotica.Web.Http.Requets.Metadata.Deezer.Models
{
    public class DeezerSong
    {
        public string Id { get; set; }
        public bool Readable { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        [JsonProperty("disk_number")]
        public int? DiskNumber { get; set; }
        [JsonProperty("track_position")]
        public int? TrackPosition { get; set; }
        public int Duration { get; set; }
        public int Rank { get; set; }

        [JsonProperty("explicit_lyrics")]
        public bool ExplicitLyrics { get; set; }

        public string Preview { get; set; }
        public List<DeezerArtist> Contributors { get; set; }
        public DeezerArtist Artist { get; set; }
        public DeezerAlbum Album { get; set; }
        public string Type { get; set; }
    }
}