using System.Collections.Generic;

namespace Audiotica.Web.Http.Requets.Metadata.Spotify.Models
{
    public class PlainTrackResponse : BasicModel
    {
        public List<FullTrack> Tracks { get; set; }
    }
}