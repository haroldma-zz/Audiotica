using System.Collections.Generic;

namespace Audiotica.Web.Models
{
    public class WebSong : WebItem
    {
        public string Title { get; set; }
        public List<WebArtist> Artists { get; set; }
        public WebAlbum Album { get; set; }
        public int TrackNumber { get; set; }
    }
}