using Newtonsoft.Json;

namespace Audiotica.Data.Model.Spotify.Models
{
    public class ErrorResponse
    {
        [JsonProperty("error")]
        public Error Error { get; set; }
    }
}