#region

using System;
using SQLite;

#endregion

namespace Audiotica.Collection.Model
{
    public class Song : BaseDbEntry
    {
        [Indexed]
        public string XboxId { get; set; }

        [Indexed]
        public string LastFmId { get; set; }


        public int ArtistId { get; set; }

        public int AlbumId { get; set; }

        public string Name { get; set; }

        public int TrackNumber { get; set; }

        public string AudioUrl { get; set; }

        public SongState SongState { get; set; }

        public int PlayCount { get; set; }

        public HeartState HeartState { get; set; }

        [Ignore]
        public bool IsStreaming
        {
            get { return new Uri(AudioUrl).IsAbsoluteUri; }
        }

        [Ignore]
        public Artist Artist { get; set; }

        [Ignore]
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