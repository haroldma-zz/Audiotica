using System;
using System.Collections.Generic;

namespace Audiotica.Web.Models
{
    public class WebAlbum : WebItemWithTracks
    {
        public WebAlbum(Type provider) : base(provider)
        {
        }

        public string Title { get; set; }
        public WebArtist Artist { get; set; }
        public DateTime? ReleasedDate { get; set; }
        public Uri Artwork { get; set; }
        public List<string> Genres{ get; set; }
    }
}