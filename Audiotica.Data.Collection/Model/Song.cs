#region

using System;
using System.Collections.ObjectModel;
using Audiotica.Data.Collection.SqlHelper;
using SQLite;

#endregion

namespace Audiotica.Data.Collection.Model
{
    public class Song : BaseEntry
    {
        public Song()
        {
            AddableTo = new ObservableCollection<AddableCollectionItem>();
        }

        private BackgroundDownload _download;
        private SongState _songState;
        public string ProviderId { get; set; }

        [Indexed]
        public int ArtistId { get; set; }

        [Indexed]
        public int AlbumId { get; set; }

        public string Name { get; set; }

        //Artist prop is for the album (main), this one is specific to each song
        public string ArtistName { get; set; }

        public int TrackNumber { get; set; }

        public string AudioUrl { get; set; }

        public SongState SongState
        {
            get { return _songState; }
            set
            {
                _songState = value;
                OnPropertyChanged();
            }
        }

        public int PlayCount { get; set; }

        public DateTime LastPlayed { get; set; }

        public HeartState HeartState { get; set; }

        public TimeSpan Duration { get; set; }


        [Ignore]
        public bool IsStreaming
        {
            get
            {
                return SongState != SongState.Downloaded
                       && SongState != SongState.Local;
            }
        }

        [Ignore]
        public Artist Artist { get; set; }

        [Ignore]
        public Album Album { get; set; }

        [Ignore]
        public BackgroundDownload Download
        {
            get { return _download; }
            set
            {
                _download = value;
                OnPropertyChanged();
            }
        }

        public string DownloadId { get; set; }

        [Ignore]
        public ObservableCollection<AddableCollectionItem> AddableTo { get; set; }
    }

    public class AddableCollectionItem
    {
        public string Name { get; set; }
        public Playlist Playlist { get; set; }
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
        Local,
        Temp
        //still playing with different states
    }
}