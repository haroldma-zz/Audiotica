﻿#region

using System;
using System.Collections.ObjectModel;
using Audiotica.Data.Collection.SqlHelper;

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

        [SqlProperty(ReferenceTo = typeof (Artist))]
        public long ArtistId { get; set; }

        [SqlProperty(ReferenceTo = typeof (Album))]
        public long AlbumId { get; set; }

        public string Name { get; set; }

        //Artist prop is for the album (main), this one is specific to each song
        public string ArtistName { get; set; }

        public long TrackNumber { get; set; }

        public string AudioUrl { get; set; }

        public SongState SongState
        {
            get { return _songState; }
            set { Set(ref _songState, value); }
        }

        public long PlayCount { get; set; }

        [SqlProperty(IsNull = true)]
        public DateTime LastPlayed { get; set; }

        public HeartState HeartState { get; set; }

        public TimeSpan Duration { get; set; }


        [SqlIgnore]
        public bool IsStreaming
        {
            get
            {
                return SongState != SongState.Downloaded
                       && SongState != SongState.Local;
            }
        }

        public Artist Artist { get; set; }

        public Album Album { get; set; }

        public BackgroundDownload Download
        {
            get { return _download; }
            set { Set(ref _download, value); }
        }

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