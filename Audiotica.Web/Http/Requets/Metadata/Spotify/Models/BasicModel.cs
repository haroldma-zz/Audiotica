using System;
using Newtonsoft.Json;

namespace Audiotica.Data.Spotify.Models
{
    public abstract class BasicModel
    {
        [JsonProperty("error")]
        public Error ErrorResponse { get; set; }

        public bool HasError()
        {
            return ErrorResponse != null;
        }
    }
}
