using System.Collections.Generic;

namespace Audiotica.Data.Model.Spotify.Models
{
    public class CheckUserTracks : BasicModel
    {
        public List<bool> Checked { get; set; }
    }
}