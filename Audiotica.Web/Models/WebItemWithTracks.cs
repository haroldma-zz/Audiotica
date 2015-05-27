using System.Collections.Generic;

namespace Audiotica.Web.Models
{
    public class WebItemWithTracks : WebItem
    {
        public List<WebSong> Tracks { get; set; }
    }
}