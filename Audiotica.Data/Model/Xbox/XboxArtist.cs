using System.Collections.Generic;

namespace Audiotica.Data.Model.Xbox
{
    public class XboxArtist : EntryBase
    {
        /// <summary>
        /// The list of musical genres associated with this artist.
        /// </summary>
        public List<string> Genres { get; set; }

        /// <summary>
        /// A list of musical sub-genres associated with the artist.
        /// </summary>
        public List<string> Subgenres { get; set; }

        /// <summary>
        /// An optional paginated list of related artists. 
        /// </summary>
        /// <remarks>
        /// This list is null by default unless requested as extra information in a lookup request. Artists in this list contain 
        /// only a few fields, including the ID that should be used in a lookup request in order to have the full artist properties.
        /// </remarks>
        public List<XboxArtist> RelatedArtists { get; set; }

        /// <summary>
        /// An optional paginated list of the artist's albums, ordered by decreasing order of release date (latest first).
        /// </summary>
        /// <remarks>
        /// This list is null by default unless requested as extra information in a lookup request. Albums in this list contain only 
        /// a few fields, including the ID that should be used in a lookup request in order to have the full album properties.
        /// </remarks>
        public List<XboxAlbum> Albums { get; set; }

        /// <summary>
        /// A paginated list of the artist's top tracks, ordered by decreasing order of popularity. 
        /// </summary>
        /// <remarks>
        /// This list is null by default unless requested as extra information in a lookup request. Tracks in this list contain 
        /// only a few fields, including the ID that should be used in a lookup request in order to have the full track properties.
        /// </remarks>
        public List<XboxTrack> TopTracks { get; set; }
    }
}