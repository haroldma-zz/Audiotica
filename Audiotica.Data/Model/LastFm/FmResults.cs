#region

using System.Collections.Generic;

#endregion

namespace Audiotica.Data.Model.LastFm
{
    public class FmArtistResults
    {
        public List<FmArtist> artist { get; set; }
    }

    public class FmTrackResults
    {
        public List<FmDetailTrack> track { get; set; }
    }
}