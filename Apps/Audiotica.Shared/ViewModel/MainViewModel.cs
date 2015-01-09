#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Model;
using Audiotica.Data.Service.Interfaces;
using Audiotica.Data.Spotify.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly AudioPlayerHelper _audioPlayer;
        private readonly ICollectionService _collectionService;
        private readonly IScrobblerService _service;
        private readonly ISpotifyService _spotify;
        private bool _isLastfmEnabled;
        private bool _isMostStreamedEnabled;
        private bool _isMostStreamedLoading;
        private bool _isRecommendationLoading;
        private List<Song> _mostPlayed = new List<Song>();
        private List<LastArtist> _recommendedArtists;
        private List<ChartTrack> _topTracks;

        /// <summary>
        ///     Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(ICollectionService collectionService, IScrobblerService service, ISpotifyService spotify,
            AudioPlayerHelper audioPlayer)
        {
            _collectionService = collectionService;
            _service = service;
            _spotify = spotify;
            _audioPlayer = audioPlayer;
            _collectionService.LibraryLoaded += CollectionServiceOnLibraryLoaded;
            _collectionService.Songs.CollectionChanged += SongsOnCollectionChanged;
            _audioPlayer.TrackChanged += CollectionServiceOnLibraryLoaded;

            //Load data automatically
            LoadChartDataAsync();
        }

        public bool IsMostStreamedLoading
        {
            get { return _isMostStreamedLoading; }
            set { Set(ref _isMostStreamedLoading, value); }
        }

        public bool IsRecommendationLoading
        {
            get { return _isRecommendationLoading; }
            set { Set(ref _isRecommendationLoading, value); }
        }

        public List<Song> MostPlayed
        {
            get { return _mostPlayed; }
            set { Set(ref _mostPlayed, value); }
        }

        public List<LastArtist> RecommendedArtists
        {
            get { return _recommendedArtists; }
            set { Set(ref _recommendedArtists, value); }
        }

        public List<ChartTrack> TopTracks
        {
            get { return _topTracks; }
            set { Set(ref _topTracks, value); }
        }

        public bool IsLastfmEnabled
        {
            get { return _isLastfmEnabled; }
            set { Set(ref _isLastfmEnabled, value); }
        }

        public bool IsMostStreamedEnabled
        {
            get { return _isMostStreamedEnabled; }
            set { Set(ref _isMostStreamedEnabled, value); }
        }

        private void SongsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                CollectionServiceOnLibraryLoaded(null, null);
            }
        }

        private async void CollectionServiceOnLibraryLoaded(object sender, EventArgs eventArgs)
        {          
            MostPlayed = _collectionService.Songs.ToList()
                .Where(p => p.PlayCount != 0 && (DateTime.Now - p.LastPlayed).TotalDays <= 14)
                .OrderByDescending(p => p.PlayCount)
                .Take(10).ToList();
        }

        public async Task LoadChartDataAsync()
        {
            var rnd = new Random(DateTime.Now.Millisecond);

            try
            {
                IsMostStreamedLoading = true;
                var page = rnd.Next(0, 9)*10;
                TopTracks = (await _spotify.GetMostStreamedTracksAsync())
                    .Skip(page).Take(10).ToList();
            }
            catch
            {
                TopTracks = new List<ChartTrack>();
            }
            IsMostStreamedLoading = false;

            _service.AuthStateChanged += ServiceOnAuthStateChanged;
            if (_service.HasCredentials)
                ServiceOnAuthStateChanged(null, new BoolEventArgs(true));
        }

        private async void ServiceOnAuthStateChanged(object sender, BoolEventArgs b)
        {
            IsLastfmEnabled = b.Content;
            IsRecommendationLoading = b.Content;
            RecommendedArtists = null;
            if (!b.Content)
                return;

            try
            {
                var rec = await _service.GetRecommendedArtistsAsync(limit: 10);
                RecommendedArtists = rec.Content;
            }
            catch
            {
            }

            if (RecommendedArtists == null)
            {
                //this will show the "no recommendation" text block
                RecommendedArtists = new List<LastArtist>();
            }

            IsRecommendationLoading = false;
        }
    }
}