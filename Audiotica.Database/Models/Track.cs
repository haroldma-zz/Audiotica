using System;
using System.Collections.Generic;
using System.Threading;
using Audiotica.Core.Common;
using Audiotica.Core.Extensions;
using Newtonsoft.Json;
using SQLite.Net.Attributes;

namespace Audiotica.Database.Models
{
    public enum TrackType
    {
        Local,
        Stream,
        Download
    }

    public enum TrackStatus
    {
        None,
        NotAvailable,
        NoMatch,
        Downloading,
        Matching
    }

    /// <summary>
    ///     Track object, used in database.
    /// </summary>
    public class Track : DatabaseEntryBase
    {
        private BackgroundDownload _backgroundDownload;
        private bool _isFromLibrary;
        private TrackStatus _status;
        private TrackType _type;

        /// <summary>
        ///     Gets or sets the album artist.
        /// </summary>
        /// <value>
        ///     The album artist.
        /// </value>
        public string AlbumArtist { get; set; }

        /// <summary>
        ///     Gets or sets the album.
        /// </summary>
        /// <value>
        ///     The album.
        /// </value>
        public string AlbumTitle { get; set; }

        /// <summary>
        ///     Gets or sets the artist artwork URI.
        /// </summary>
        /// <value>
        ///     The artist artwork URI.
        /// </value>
        public string ArtistArtworkUri { get; set; }

        /// <summary>
        ///     Gets or sets the artists, using ';' delimiter.
        /// </summary>
        /// <value>
        ///     The artists.
        /// </value>
        public string Artists { get; set; }

        /// <summary>
        ///     Gets or sets the artwork URI.
        /// </summary>
        /// <value>
        ///     The artwork URI.
        /// </value>
        public string ArtworkUri { get; set; }

        /// <summary>
        ///     Gets or sets the local audio URI.
        /// </summary>
        /// <value>
        ///     The audio local URI.
        /// </value>
        public string AudioLocalUri { get; set; }

        /// <summary>
        ///     Gets or sets the web audio URI.
        ///     Blank if is local.
        /// </summary>
        /// <value>
        ///     The URI.
        /// </value>
        public string AudioWebUri { get; set; }

        /// <summary>
        ///     Gets or sets the background download.
        /// </summary>
        /// <value>
        ///     The current background download.
        /// </value>
        [Ignore, JsonIgnore]
        public BackgroundDownload BackgroundDownload
        {
            get
            {
                return _backgroundDownload;
            }
            set
            {
                Set(ref _backgroundDownload, value);
            }
        }

        /// <summary>
        ///     Gets or sets the bitrate.
        /// </summary>
        /// <value>
        ///     The bitrate.
        /// </value>
        public int Bitrate { get; set; }

        /// <summary>
        ///     Gets or sets the channels.
        /// </summary>
        /// <value>
        ///     The channels.
        /// </value>
        public int Channels { get; set; }

        /// <summary>
        ///     Gets or sets the comment.
        /// </summary>
        /// <value>
        ///     The comment.
        /// </value>
        public string Comment { get; set; }

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
        ///     Gets or sets the disc count.
        /// </summary>
        /// <value>
        ///     The disc count.
        /// </value>
        public uint DiscCount { get; set; }

        /// <summary>
        ///     Gets or sets the disc number.
        /// </summary>
        /// <value>
        ///     The disc number.
        /// </value>
        public uint DiscNumber { get; set; }

        /// <summary>
        ///     Gets or sets the display artist.
        /// </summary>
        /// <value>
        ///     The display artist.
        /// </value>
        public string DisplayArtist { get; set; }

        public TimeSpan Duration { get; set; }

        /// <summary>
        ///     Gets or sets the size of the file, in bytes.
        /// </summary>
        /// <value>
        ///     The size of the file.
        /// </value>
        public long FileSize { get; set; }

        /// <summary>
        ///     Gets or sets the genre.
        /// </summary>
        /// <value>
        ///     The genre.
        /// </value>
        public string Genres { get; set; }

        [Ignore, JsonIgnore]
        public bool IsDownloadable => Type == TrackType.Stream && Status == TrackStatus.None && IsFromLibrary;

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is from the music library.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is from the library; otherwise, <c>false</c>.
        /// </value>
        [Ignore]
        public bool IsFromLibrary
        {
            get
            {
                return _isFromLibrary;
            }
            set
            {
                Set(ref _isFromLibrary, value);
                RaisePropertyChanged("IsDownloadable");
            }
        }

        /// <summary>
        ///     Gets or sets the last played.
        /// </summary>
        /// <value>
        ///     The last played.
        /// </value>
        public DateTime? LastPlayed { get; set; }

        public LikeState LikeState { get; set; }

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
        ///     Gets or sets the publisher.
        /// </summary>
        /// <value>
        ///     The publisher.
        /// </value>
        public string Publisher { get; set; }

        /// <summary>
        ///     Gets or sets the sample rate.
        /// </summary>
        /// <value>
        ///     The sample rate.
        /// </value>
        public int SampleRate { get; set; }

        public TrackStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                Set(ref _status, value);
                RaisePropertyChanged("IsDownloadable");
            }
        }

        /// <summary>
        ///     Gets or sets the track's title.
        /// </summary>
        /// <value>
        ///     The title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        ///     Gets or sets the track count.
        /// </summary>
        /// <value>
        ///     The track count.
        /// </value>
        public uint TrackCount { get; set; }

        /// <summary>
        ///     Gets or sets the track number.
        /// </summary>
        /// <value>
        ///     The track number.
        /// </value>
        public uint TrackNumber { get; set; }

        /// <summary>
        ///     Gets or sets the track type.
        /// </summary>
        /// <value>
        ///     The type.
        /// </value>
        public TrackType Type
        {
            get
            {
                return _type;
            }
            set
            {
                Set(ref _type, value);
                RaisePropertyChanged("IsDownloadable");
            }
        }

        /// <summary>
        ///     Gets or sets the year.
        /// </summary>
        /// <value>
        ///     The year.
        /// </value>
        public uint? Year { get; set; }

        public override string ToString()
        {
            return $"{Title} by {DisplayArtist}";
        }
    }

    public enum LikeState
    {
        None,
        Like,
        Dislike
    }

    public class BackgroundDownload : ObservableObject
    {
        private double _bytesReceived;
        private double _bytesToReceive = 1;
        private string _status = "Waiting";

        public BackgroundDownload(object downloadOperation)
        {
            DownloadOperation = downloadOperation;
            CancellationTokenSrc = new CancellationTokenSource();
        }

        public double BytesReceived
        {
            get
            {
                return _bytesReceived;
            }
            set
            {
                Set(ref _bytesReceived, value);
            }
        }

        public double BytesToReceive
        {
            get
            {
                return _bytesToReceive;
            }
            set
            {
                Set(ref _bytesToReceive, value);
            }
        }

        public CancellationTokenSource CancellationTokenSrc { get; }

        public object DownloadOperation { get; }

        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                Set(ref _status, value);
            }
        }
    }

    public class TrackComparer : IEqualityComparer<Track>
    {
        public static bool AreEqual(Track x, Track y)
        {
            return new TrackComparer().Equals(x, y);
        }

        public static string GetSlug(Track track)
        {
            return (track.Title + track.DisplayArtist
                + track.AlbumArtist
                + track.AlbumTitle).ToAudioticaSlug();
        }

        public bool Equals(Track x, Track y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            if (x.IsFromLibrary && y.IsFromLibrary)
            {
                return x.Id == y.Id;
            }
            return GetHashCode(x) == GetHashCode(y);
        }

        public int GetHashCode(Track obj)
        {
            return GetSlug(obj).GetHashCode();
        }
    }
}