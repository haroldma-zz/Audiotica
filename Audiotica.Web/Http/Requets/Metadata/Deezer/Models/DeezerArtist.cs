using Newtonsoft.Json;

namespace Audiotica.Web.Http.Requets.Metadata.Deezer.Models
{
    public class DeezerArtist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Tracklist { get; set; }
        public string Type { get; set; }

        [JsonProperty("picture_big")]
        public string PictureBig { get; set; }
    }
}