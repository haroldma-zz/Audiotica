using System.Collections.Generic;
using System.Linq;
using Audiotica.Core.Common;
using Audiotica.Data.Service.Interfaces;
using GalaSoft.MvvmLight;
using IF.Lastfm.Core.Objects;

namespace Audiotica.ViewModel
{
    public class ArtistRecommendationsViewModel : ViewModelBase
    {
        private readonly IScrobblerService _scrobbler;
        private bool _isRecommendationLoading;
        private List<LastArtist> _recommendedArtists;
        private bool _isLastFmEnabled;

        public ArtistRecommendationsViewModel(IScrobblerService scrobbler)
        {
            _scrobbler = scrobbler;
            scrobbler.AuthStateChanged += ServiceOnAuthStateChanged;
            if (scrobbler.HasCredentials)
            {
                ServiceOnAuthStateChanged(null, new BoolEventArgs(true));
            }
        }

        public List<LastArtist> RecommendedArtists
        {
            get { return _recommendedArtists; }
            set { Set(ref _recommendedArtists, value); }
        }

        public bool IsRecommendationLoading
        {
            get { return _isRecommendationLoading; }
            set { Set(ref _isRecommendationLoading, value); }
        }

        private async void ServiceOnAuthStateChanged(object sender, BoolEventArgs b)
        {
            IsRecommendationLoading = b.Content;
            IsLastfmEnabled = b.Content;
            RecommendedArtists = null;
            if (!b.Content)
            {
                return;
            }

            try
            {
                var rec = await _scrobbler.GetRecommendedArtistsAsync(limit: 4);
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

        public bool IsLastfmEnabled
        {
            get { return _isLastFmEnabled; }
            set { Set(ref _isLastFmEnabled, value); }
        }
    }
}