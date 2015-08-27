using Newtonsoft.Json;

namespace Audiotica.Web.Models.Vk
{
    public class VkSong
    {
        public int Aid { get; set; }

        [JsonProperty("owner_id")]
        public int OwnerId { get; set; }

        public string Artist { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public string Url { get; set; }

        [JsonProperty("lyrics_id")]
        public string LyricsId { get; set; }

        public int Genre { get; set; }
    }
}