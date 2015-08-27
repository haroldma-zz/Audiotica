using Newtonsoft.Json;

namespace Audiotica.Web.Http.Requets.MatchEngine.SoundCloud.Models
{
    public class SoundCloudUser
    {
        public int Id { get; set; }

        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }

        public string Kind { get; set; }
        public string Username { get; set; }
        public string Uri { get; set; }

        [JsonProperty("permalink_url")]
        public string PermalinkUrl { get; set; }

        public string Permalink { get; set; }
    }
}