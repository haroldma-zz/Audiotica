using System.Collections.Generic;

namespace Audiotica.Web.Models
{
    public class WebResults
    {
        public enum Type
        {
            Song,
            Artist,
            Album
        }

        public string PageToken { get; set; }
        public bool HasMore { get; set; }
        public List<WebSong> Songs { get; set; }
        public List<WebArtist> Artists { get; set; }
        public List<WebAlbum> Albums { get; set; }
    }
}