using System;

namespace Audiotica.Web.Models
{
    public class WebArtist : WebItem
    {
        public string Name { get; set; }
        public Uri Artwork { get; set; }
    }
}