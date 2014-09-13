using System.Collections.Generic;

namespace Audiotica.Data.Model.SoundCloud
{
    public class SoundCloudRoot
    {
        public List<SoundCloudSong> collection { get; set; }
        public string next_href { get; set; }
    }
}
