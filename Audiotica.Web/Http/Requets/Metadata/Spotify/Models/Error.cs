using Newtonsoft.Json;

namespace Audiotica.Web.Http.Requets.Metadata.Spotify.Models
{
    public class Error
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}