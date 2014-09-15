#region

using System;
using SQLite;

#endregion

namespace Audiotica.Collection.Model
{
    public class Song : BaseDbEntry
    {
        public int ArtistId { get; set; }

        public int AlbumId { get; set; }

        public string Name { get; set; }

        public Uri AudioUri { get; set; }

        public SongState SongState { get; set; }

        public int PlayCount { get; set; }

        public HeartState HeartState { get; set; }

        [Ignore]
        public bool IsStreaming
        {
            get { return AudioUri.IsAbsoluteUri; }
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