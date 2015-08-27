using System;
using Newtonsoft.Json;

namespace Audiotica.Web.Http.Requets.Metadata.Deezer.Models
{
    public class DeezerAlbum
    {
        public int Id { get; set; }
        public string Title { get; set; }

        [JsonProperty("cover_big")]
        public string CoverBig { get; set; }

        public string Tracklist { get; set; }
        public string Type { get; set; }
        public DeezerArtist Artist { get; set; }
        public DataResponse<Genre> Genres { get; set; }
        public DataResponse<DeezerSong> Tracks { get; set; }

        [JsonProperty("release_date")]
        public DateTime? ReleaseDate { get; set; }

        public class Genre
        {
            public string Name { get; set; }
        }
    }
}