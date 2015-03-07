#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Audiotica.Controls.Home;
using Audiotica.Core.Common;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Model;
using Audiotica.Data.Model.AudioticaCloud;
using Audiotica.Data.Service.Interfaces;
using Audiotica.Data.Spotify.Models;
using GalaSoft.MvvmLight;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly AudioPlayerHelper _audioPlayer;

        private readonly IAudioticaService _audioticaService;

        private readonly ICollectionService _collectionService;

        private readonly IScrobblerService _service;

        private readonly ISpotifyService _spotify;

        private bool _isLastfmEnabled;

        private bool _isMostStreamedEnabled;

        private bool _isMostStreamedLoading;

        private bool _isRecommendationLoading;

        private ObservableCollection<SpotlightFeature> _largeFeatures;

        private ObservableCollection<SpotlightFeature> _mediumFeatures;

        private List<Song> _mostPlayed = new List<Song>();

        private List<LastArtist> _recommendedArtists;

        private List<ChartTrack> _topTracks;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        /// <param name="collectionService">
        /// The collection service.
        /// </param>
        /// <param name="service">
        /// The service.
        /// </param>
        /// <param name="spotify">
        /// The spotify.
        /// </param>
        /// <param name="xboxService">
        /// The xbox service.
        /// </param>
        /// <param name="audioticaService">
        /// The audiotica service.
        /// </param>
        /// <param name="audioPlayer">
        /// The audio player.
        /// </param>
        public MainViewModel(
            ICollectionService collectionService,
            IScrobblerService service,
            ISpotifyService spotify,
            IAudioticaService audioticaService,
            AudioPlayerHelper audioPlayer)
        {
            _collectionService = collectionService;
            _service = service;
            _spotify = spotify;
            _audioticaService = audioticaService;
            _audioPlayer = audioPlayer;
            _collectionService.LibraryLoaded += CollectionServiceOnLibraryLoaded;
            _collectionService.Songs.CollectionChanged += SongsOnCollectionChanged;
            audioPlayer.TrackChanged += CollectionServiceOnLibraryLoaded;

            // Load data automatically
            LoadChartDataAsync();
        }

        public bool IsMostStreamedLoading
        {
            get
            {
                return _isMostStreamedLoading;
            }

            set
            {
                Set(ref _isMostStreamedLoading, value);
            }
        }

        public bool IsRecommendationLoading
        {
            get
            {
                return _isRecommendationLoading;
            }

            set
            {
                Set(ref _isRecommendationLoading, value);
            }
        }

        public List<Song> MostPlayed
        {
            get
            {
                return _mostPlayed;
            }

            set
            {
                Set(ref _mostPlayed, value);
            }
        }

        public List<LastArtist> RecommendedArtists
        {
            get
            {
                return _recommendedArtists;
            }

            set
            {
                Set(ref _recommendedArtists, value);
            }
        }

        public ObservableCollection<SpotlightFeature> LargeFeatures
        {
            get
            {
                return _largeFeatures;
            }

            set
            {
                Set(ref _largeFeatures, value);
            }
        }

        public ObservableCollection<SpotlightFeature> MediumFeatures
        {
            get
            {
                return _mediumFeatures;
            }

            set
            {
                Set(ref _mediumFeatures, value);
            }
        }

        public List<ChartTrack> TopTracks
        {
            get
            {
                return _topTracks;
            }

            set
            {
                Set(ref _topTracks, value);
            }
        }

        public bool IsLastfmEnabled
        {
            get
            {
                return _isLastfmEnabled;
            }

            set
            {
                Set(ref _isLastfmEnabled, value);
            }
        }

        public bool IsMostStreamedEnabled
        {
            get
            {
                return _isMostStreamedEnabled;
            }

            set
            {
                Set(ref _isMostStreamedEnabled, value);
            }
        }

        public async Task LoadChartDataAsync()
        {
            LoadSpotlight();

            var rnd = new Random(DateTime.Now.Millisecond);

            try
            {
                IsMostStreamedLoading = true;
                var page = rnd.Next(0, 9) * 10;
                TopTracks = (await _spotify.GetMostStreamedTracksAsync()).Skip(page).Take(10).ToList();
            }
            catch
            {
                TopTracks = new List<ChartTrack>();
            }

            IsMostStreamedLoading = false;

            _service.AuthStateChanged += ServiceOnAuthStateChanged;
            if (_service.HasCredentials)
            {
                ServiceOnAuthStateChanged(null, new BoolEventArgs(true));
            }
        }

        private void CollectionServiceOnLibraryLoaded(object sender, EventArgs eventArgs)
        {
            MostPlayed =
                _collectionService.Songs.ToList()
                    .Where(p => p.PlayCount != 0 && (DateTime.Now - p.LastPlayed).TotalDays <= 14)
                    .OrderByDescending(p => p.PlayCount)
                    .Take(10)
                    .ToList();

            if (MostPlayed.Count == 10)
            {
                _audioPlayer.TrackChanged -= CollectionServiceOnLibraryLoaded;
            }
        }

        private async void LoadSpotlight()
        {
            AudioticaSpotlight spotlight = null;

            try
            {
                spotlight = await _audioticaService.GetSpotlightAsync();
            }
            catch
            {
                // ignored
            }

            PageResponse<LastArtist> topArtist = null;

            try
            {
                topArtist = await _service.GetTopArtistsAsync(limit: 10);
            }
            catch
            {
                // ignored
            }

            LargeFeatures = new ObservableCollection<SpotlightFeature>();
            MediumFeatures = new ObservableCollection<SpotlightFeature>();

            if (spotlight != null)
            {
                if (spotlight.LargeFeatures != null)
                {
                    foreach (
                        var spotlightFeature in spotlight.LargeFeatures.Where(p => ShouldShow(p.ShowTo, p.ShowToNot)))
                    {
                        LargeFeatures.Add(spotlightFeature);
                    }
                }

                if (spotlight.MediumFeatures != null)
                {
                    foreach (
                        var spotlightFeature in
                            spotlight.MediumFeatures.Where(p => p.InsertAtTop && ShouldShow(p.ShowTo, p.ShowToNot)))
                    {
                        MediumFeatures.Add(spotlightFeature);
                    }
                }
            }

            if (topArtist != null && topArtist.Content != null)
            {
                foreach (var lastArtist in topArtist.Content)
                {
                    MediumFeatures.Add(
                        new SpotlightFeature
                        {
                            Title = lastArtist.Name,
                            Text = string.Format("{0:#,###} plays", lastArtist.PlayCount),
                            ImageUri = lastArtist.MainImage.Largest.AbsoluteUri,
                            Action = "artist:" + lastArtist.Name
                        });
                }
            }

            if (spotlight != null && spotlight.MediumFeatures != null)
            {
                foreach (
                    var spotlightFeature in
                        spotlight.MediumFeatures.Where(p => !p.InsertAtTop && ShouldShow(p.ShowTo, p.ShowToNot)))
                {
                    MediumFeatures.Add(spotlightFeature);
                }
            }
            if (spotlight != null || topArtist != null)
            {
                MessengerInstance.Send(true, "spotlight");
            }
        }

        private async void ServiceOnAuthStateChanged(object sender, BoolEventArgs b)
        {
            IsLastfmEnabled = b.Content;
            IsRecommendationLoading = b.Content;
            RecommendedArtists = null;
            if (!b.Content)
            {
                return;
            }

            try
            {
                var rec = await _service.GetRecommendedArtistsAsync(limit: 10);
                RecommendedArtists = rec.Content.ToList();
            }
            catch
            {
            }

            if (RecommendedArtists == null)
            {
                // this will show the "no recommendation" text block
                RecommendedArtists = new List<LastArtist>();
            }

            IsRecommendationLoading = false;
        }

        private bool ShouldShow(ShowTo showTo, bool showToNot)
        {
            bool result;
            switch (showTo)
            {
                case ShowTo.All:
                    result = true;
                    break;
                case ShowTo.Beta:
                    result = !App.IsProduction;
                    break;
                case ShowTo.Cloud:
                    result = _audioticaService.IsAuthenticated;
                    break;
                case ShowTo.Cancelled:
                    result = _audioticaService.IsAuthenticated
                           && _audioticaService.CurrentUser.SubscriptionStatus == SubscriptionStatus.Canceled;
                    break;
                case ShowTo.Trial:
                    result = _audioticaService.IsAuthenticated
                           && _audioticaService.CurrentUser.SubscriptionStatus == SubscriptionStatus.Trialing;
                    break;
                case ShowTo.ActiveSubscription:
                    result = _audioticaService.IsAuthenticated
                           && _audioticaService.CurrentUser.SubscriptionStatus == SubscriptionStatus.Active;
                    break;
                case ShowTo.PastDue:
                    result = _audioticaService.IsAuthenticated
                           && _audioticaService.CurrentUser.SubscriptionStatus == SubscriptionStatus.PastDue;
                    break;
                case ShowTo.ScrobblingEnabled:
                    result = App.Locator.ScrobblerService.IsAuthenticated;
                    break;
                default:
                    return false;
            }

            if (showToNot)
            {
                return !result;
            }

            return result;
        }

        private void SongsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                CollectionServiceOnLibraryLoaded(null, null);
            }
        }
    }
}