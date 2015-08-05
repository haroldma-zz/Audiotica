using System;

namespace Audiotica.Web.Models
{
    public class WebArtist : WebItem
    {
        public WebArtist(Type provider) : base(provider)
        {
        }

        public string Name { get; set; }
        public Uri Artwork { get; set; }
    }
}