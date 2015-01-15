#region

using Newtonsoft.Json;

#endregion

namespace Audiotica.Data.Model.AudioticaCloud
{
    public class BaseAudioticaResponse<T> : BaseAudioticaResponse
    {
        [JsonProperty(PropertyName = "data")]
        public T Data { get; set; }

        [JsonIgnore]
        public bool Success { get; set; }
    }

    public class BaseAudioticaResponse
    {
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}