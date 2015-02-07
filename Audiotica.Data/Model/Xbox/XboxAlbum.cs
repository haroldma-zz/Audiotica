using System;
using System.Collections.Generic;

namespace Audiotica.Data.Model.Xbox
{
    public class XboxAlbum : EntryBase
    {
        /// <summary>
        /// Nullable. The album release date.
        /// </summary>
        public DateTime? ReleaseDate { get; set; }

        /// <summary>
        /// Nullable. The album total duration.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// Nullable. The number of tracks on the album.
        /// </summary>
        public int? TrackCount { get; set; }

        /// <summary>
        /// Nullable. True if the album contains explicit content.
        /// </summary>
        public bool? IsExplicit { get; set; }

        /// <summary>
        /// The name of the music label that produced this album.
        /// </summary>
        public string LabelName { get; set; }

        /// <summary>
        /// The list of musical genres associated with this album.
        /// </summary>
        public List<string> Genres { get; set; }

        /// <summary>
        /// The list of musical sub-genres associated with this album.
        /// </summary>
        public List<string> Subgenres { get; set; }

        /// <summary>
        /// The type of album (for example, Album, Single, and so on).
        /// </summary>
        public string AlbumType { get; set; }

        /// <summary>
        /// The album subtitle.
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// The list of contributors (artists and their roles) to the album.
        /// </summary>
        public List<Contributor> Artists { get; set; }

        /// <summary>
        /// A paginated list of the album's tracks. 
        /// </summary>
        /// <remarks>
        /// This list is null by default unless requested as extra information in a lookup request. If not null, it should 
        /// most often be full without the need to use a continuation token; only a few cases of albums containing a very 
        /// large number of tracks will use pagination. Tracks in this list contain only a few fields, including the ID 
        /// that should be used in a lookup request in order to have the full track properties.
        /// </remarks>
        public XboxPaginatedList<XboxTrack> Tracks { get; set; }

    }
}