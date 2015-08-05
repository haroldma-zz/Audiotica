using System;
using System.Collections.Generic;

namespace Audiotica.Web.Models
{
    public class WebSong : WebItem
    {
        public WebSong(Type provider) : base(provider)
        {
        }

        public string Title { get; set; }
        public string Genres { get; set; }
        public List<WebArtist> Artists { get; set; }
        public WebAlbum Album { get; set; }
        public int TrackNumber { get; set; }
        public int DiscNumber { get; set; }
    }
}