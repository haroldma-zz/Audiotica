using System;
using System.Collections.Generic;

namespace Audiotica.Web.Models
{
    public class WebItemWithTracks : WebItem
    {
        public WebItemWithTracks(Type provider) : base(provider)
        {
        }

        public List<WebSong> Tracks { get; set; }
    }
}