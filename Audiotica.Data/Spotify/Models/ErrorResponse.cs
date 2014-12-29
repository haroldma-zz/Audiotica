using Newtonsoft.Json;

namespace Audiotica.Data.Spotify.Models
{
    public class ErrorResponse
    {
        [JsonProperty("error")]
        public Error Error { get; set; }
    }
}