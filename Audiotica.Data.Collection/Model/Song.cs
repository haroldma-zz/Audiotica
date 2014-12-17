#region

using System;
using Audiotica.Data.Collection.SqlHelper;

#endregion

namespace Audiotica.Data.Collection.Model
{
    public class Song : BaseEntry
    {
        public string ProviderId { get; set; }

        [SqlProperty(ReferenceTo = typeof (Artist))]
        public long ArtistId { get; set; }

        [SqlProperty(ReferenceTo = typeof (Album))]
        public long AlbumId { get; set; }

        public string Name { get; set; }

        //Artist prop is for the album (main), this one is specific to each song
        public string ArtistName { get; set; }

        public long TrackNumber { get; set; }

        public string AudioUrl { get; set; }

        public SongState SongState { get; set; }

        public long PlayCount { get; set; }

        [SqlProperty(IsNull = true)]
        public DateTime LastPlayed { get; set; }

        public HeartState HeartState { get; set; }

        public TimeSpan Duration { get; set; }


        [SqlIgnore]
        public bool IsStreaming
        {
            get { return new Uri(AudioUrl).IsAbsoluteUri; }
        }

        public Artist Artist { get; set; }

        public Album Album { get; set; }
    }

    public enum HeartState
    {
        None,
        Like,
        Dislike
    }

    public enum SongState
    {
        None,
        Downloading,
        Downloaded,
        Uploading
        //still playing with different states
    }
}