using System.Collections.Generic;

namespace Audiotica.Data.Spotify.Models
{
    public class CheckUserTracks : BasicModel
    {
        public List<bool> Checked { get; set; }
    }
}