using System;

namespace Audiotica.Web.Models
{
    public class WebAlbum : WebItemWithTracks
    {
        public string Name { get; set; }
        public WebArtist Artist { get; set; }
        public DateTime ReleasedDate { get; set; }
        public Uri Artwork { get; set; }
    }
}