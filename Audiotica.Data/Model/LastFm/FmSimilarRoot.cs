#region

using System.Collections.Generic;

#endregion

namespace Audiotica.Data.Model.LastFm
{
    public class FmSimilarRoot
    {
        public FmArtistResults similarartists { get; set; }
        public FmTrackResults similartracks { get; set; }
    }
}