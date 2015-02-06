#region

using System;
using System.Collections.ObjectModel;

using Audiotica.Data.Spotify.Models;

using Newtonsoft.Json;

using SQLite;

#endregion

namespace Audiotica.Data.Collection.Model
{
    public class Song : BaseEntry
    {
        private BackgroundDownload _download;

        private SongState _songState;

        public Song()
        {
            SongState = SongState.Matching;
            AddableTo = new ObservableCollection<AddableCollectionItem>();
        }

        public Song(CloudSong cloud)
            : this()
        {
            Name = cloud.Name;
            ArtistName = cloud.ArtistName;
            ProviderId = cloud.ProviderId;
            TrackNumber = cloud.TrackNumber;
            Duration = cloud.Duration;
            HeartState = cloud.HeartState;
            PlayCount = cloud.PlayCount;
            LastPlayed = cloud.LastPlayed;
            CloudId = cloud.Id;
        }

        public string ProviderId { get; set; }

        [Indexed]
        public int ArtistId { get; set; }

        [Indexed]
        public int AlbumId { get; set; }

        public string Name { get; set; }

        // Artist prop is for the album (main), this one is specific to each song
        public string ArtistName { get; set; }

        public int TrackNumber { get; set; }

        public string AudioUrl { get; set; }

        [JsonIgnore]
        public SongState SongState
        {
            get
            {
                return _songState;
            }

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

        [JsonIgnore]
        public string CloudId { get; set; }

        [JsonIgnore]
        public DateTime LastUpdated { get; set; }

        [Ignore]
        [JsonIgnore]
        public bool IsStreaming
        {
            get
            {
                return SongState != SongState.Downloaded && SongState != SongState.Local;
            }
        }

        [Ignore]
        [JsonIgnore]
        public Artist Artist { get; set; }

        [Ignore]
        [JsonIgnore]
        public Album Album { get; set; }

        [Ignore]
        [JsonIgnore]
        public BackgroundDownload Download
        {
            get
            {
                return _download;
            }

            set
            {
                _download = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public string DownloadId { get; set; }

        [Ignore]
        [JsonIgnore]
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

        Temp, 

        Matching, 

        NoMatch

        // still playing with different states
    }
}