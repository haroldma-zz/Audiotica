using System;
using System.Collections.Generic;

namespace Audiotica.Data.Model.Xbox
{
    public class XboxTrack : EntryBase
    {
        /// <summary>
        /// Nullable. The track duration.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// Nullable. The position of the track in the album.
        /// </summary>
        public int? TrackNumber { get; set; }

        /// <summary>
        /// Nullable. True if the track contains explicit content.
        /// </summary>
        public bool? IsExplicit { get; set; }

        /// <summary>
        /// The list of musical genres associated with this track.
        /// </summary>
        public List<string> Genres { get; set; }

        /// <summary>
        /// The list of distribution rights associated with this track in Xbox Music (for example, Stream, Purchase, and so on).
        /// </summary>
        public List<string> Rights { get; set; }

        /// <summary>
        /// The track's subtitle.
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// The album this track belongs to.
        /// </summary>
        /// <remarks>
        /// Only a few fields are populated in this Album element, including the ID that should be used in a lookup request in order to have the full album properties.
        /// </remarks>
        public XboxAlbum Album { get; set; }

        /// <summary>
        /// The list of contributors (artists and their roles) to the album.
        /// </summary>
        public List<Contributor> Artists { get; set; }
    }
}