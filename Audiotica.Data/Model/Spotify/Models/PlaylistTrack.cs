using System;
using Newtonsoft.Json;

namespace Audiotica.Data.Model.Spotify.Models
{
    public class PlaylistTrack
    {
        [JsonProperty("added_at")]
        public DateTime AddedAt { get; set; }
        [JsonProperty("added_by")]
        public PublicProfile AddedBy { get; set; }
        [JsonProperty("track")]
        public FullTrack Track { get; set; }
    }
}