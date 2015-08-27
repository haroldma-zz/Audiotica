using Newtonsoft.Json;

namespace Audiotica.Database.Models
{
    /// <summary>
    ///     Not used in db.
    /// </summary>
    public class PlaylistTrack
    {
        public string Id { get; set; }
        public string TrackId { get; set; }

        [JsonIgnore]
        public Track Track { get; set; }
    }
}