using System;

namespace Audiotica.Database.Models
{
    /// <summary>
    ///     Track object, used in database.
    /// </summary>
    public class Track : DatabaseEntryBase
    {
        public enum TrackType
        {
            Local,
            Stream,
            Download
        }

        /// <summary>
        ///     Gets or sets the track's title.
        /// </summary>
        /// <value>
        ///     The title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        ///     Gets or sets the artists, using ';' delimiter.
        /// </summary>
        /// <value>
        ///     The artists.
        /// </value>
        public string Artists { get; set; }

        /// <summary>
        ///     Gets or sets the display artist.
        /// </summary>
        /// <value>
        ///     The display artist.
        /// </value>
        public string DisplayArtist { get; set; }

        /// <summary>
        ///     Gets or sets the album.
        /// </summary>
        /// <value>
        ///     The album.
        /// </value>
        public string AlbumTitle { get; set; }

        /// <summary>
        ///     Gets or sets the year.
        /// </summary>
        /// <value>
        ///     The year.
        /// </value>
        public int Year { get; set; }

        /// <summary>
        ///     Gets or sets the track number.
        /// </summary>
        /// <value>
        ///     The track number.
        /// </value>
        public int TrackNumber { get; set; }

        /// <summary>
        ///     Gets or sets the track count.
        /// </summary>
        /// <value>
        ///     The track count.
        /// </value>
        public int TrackCount { get; set; }

        /// <summary>
        ///     Gets or sets the disc number.
        /// </summary>
        /// <value>
        ///     The disc number.
        /// </value>
        public int DiscNumber { get; set; }

        /// <summary>
        ///     Gets or sets the disc count.
        /// </summary>
        /// <value>
        ///     The disc count.
        /// </value>
        public int DiscCount { get; set; }

        /// <summary>
        ///     Gets or sets the genre.
        /// </summary>
        /// <value>
        ///     The genre.
        /// </value>
        public string Genres { get; set; }

        /// <summary>
        ///     Gets or sets the comment.
        /// </summary>
        /// <value>
        ///     The comment.
        /// </value>
        public string Comment { get; set; }

        /// <summary>
        ///     Gets or sets the web audio URI.
        ///     Blank if is local.
        /// </summary>
        /// <value>
        ///     The URI.
        /// </value>
        public Uri AudioWebUri { get; set; }

        /// <summary>
        ///     Gets or sets the local audio URI.
        /// </summary>
        /// <value>
        ///     The audio local URI.
        /// </value>
        public Uri AudioLocalUri { get; set; }

        /// <summary>
        ///     Gets or sets the album artist.
        /// </summary>
        /// <value>
        ///     The album artist.
        /// </value>
        public string AlbumArtist { get; set; }

        /// <summary>
        ///     Gets or sets the composers.
        /// </summary>
        /// <value>
        ///     The composers, using ';' delimiter.
        /// </value>
        public string Composers { get; set; }

        /// <summary>
        ///     Gets or sets the conductors.
        /// </summary>
        /// <value>
        ///     The conductors.
        /// </value>
        public string Conductors { get; set; }

        /// <summary>
        ///     Gets or sets the copyright.
        /// </summary>
        /// <value>
        ///     The copyright.
        /// </value>
        public string Copyright { get; set; }

        /// <summary>
        ///     Gets or sets the publisher.
        /// </summary>
        /// <value>
        ///     The publisher.
        /// </value>
        public string Publisher { get; set; }

        /// <summary>
        ///     Gets or sets the lyrics.
        /// </summary>
        /// <value>
        ///     The lyrics.
        /// </value>
        public string Lyrics { get; set; }

        /// <summary>
        ///     Gets or sets the play count.
        /// </summary>
        /// <value>
        ///     The play count.
        /// </value>
        public int PlayCount { get; set; }

        /// <summary>
        ///     Gets or sets the bitrate.
        /// </summary>
        /// <value>
        ///     The bitrate.
        /// </value>
        public int Bitrate { get; set; }

        /// <summary>
        ///     Gets or sets the sample rate.
        /// </summary>
        /// <value>
        ///     The sample rate.
        /// </value>
        public int SampleRate { get; set; }

        /// <summary>
        ///     Gets or sets the channels.
        /// </summary>
        /// <value>
        ///     The channels.
        /// </value>
        public int Channels { get; set; }

        /// <summary>
        ///     Gets or sets the size of the file, in bytes.
        /// </summary>
        /// <value>
        ///     The size of the file.
        /// </value>
        public long FileSize { get; set; }

        /// <summary>
        ///     Gets or sets the last played.
        /// </summary>
        /// <value>
        ///     The last played.
        /// </value>
        public DateTime? LastPlayed { get; set; }

        /// <summary>
        ///     Gets or sets the track type.
        /// </summary>
        /// <value>
        ///     The type.
        /// </value>
        public TrackType Type { get; set; }

        /// <summary>
        ///     Gets or sets the artwork URI.
        /// </summary>
        /// <value>
        ///     The artwork URI.
        /// </value>
        public Uri ArtworkUri { get; set; }

        /// <summary>
        ///     Gets or sets the artist artwork URI.
        /// </summary>
        /// <value>
        ///     The artist artwork URI.
        /// </value>
        public Uri ArtistArtworkUri { get; set; }
    }
}