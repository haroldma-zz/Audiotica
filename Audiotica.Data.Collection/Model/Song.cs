#region

using System;

#endregion

namespace Audiotica.Data.Collection.Model
{
    public class Song
    {
        public long Id { get; set; }

        public string XboxId { get; set; }

        public string LastFmId { get; set; }


        public long ArtistId { get; set; }

        public long AlbumId { get; set; }

        public string Name { get; set; }

        public long TrackNumber { get; set; }

        public string AudioUrl { get; set; }

        public SongState SongState { get; set; }

        public long PlayCount { get; set; }

        public HeartState HeartState { get; set; }

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