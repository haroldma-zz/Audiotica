#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Exceptions;
using Audiotica.Core.Utilities;
using Audiotica.Data.Service.Interfaces;
using GalaSoft.MvvmLight;
using GoogleAnalytics;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly AudioPlayerHelper _audioPlayer;
        private readonly IScrobblerService _service;
        private bool _isFeaturedLoading;
        private bool _isNewLoading;
        private bool _isSliderLoading;
        private List<LastArtist> _spotlightItems;
        private List<LastTrack> _topTracks;

        /// <summary>
        ///     Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IScrobblerService service, AudioPlayerHelper audioPlayer)
        {
            _service = service;
            _audioPlayer = audioPlayer;

            //Load data automatically
            LoadChartDataAsync();
        }

        public bool IsFeaturedLoading
        {
            get { return _isFeaturedLoading; }
            set { Set(ref _isFeaturedLoading, value); }
        }

        public bool IsSliderLoading
        {
            get { return _isSliderLoading; }
            set { Set(ref _isSliderLoading, value); }
        }

        public bool IsNewLoading
        {
            get { return _isNewLoading; }
            set { Set(ref _isNewLoading, value); }
        }

        public List<LastArtist> SpotlightItems
        {
            get { return _spotlightItems; }
            set { Set(ref _spotlightItems, value); }
        }

        public List<LastTrack> TopTracks
        {
            get { return _topTracks; }
            set { Set(ref _topTracks, value); }
        }

        public async Task LoadChartDataAsync()
        {
            IsSliderLoading = true;
            IsFeaturedLoading = true;
            IsNewLoading = true;

            var rnd = new Random(DateTime.Now.Millisecond);

            try
            {
                var page = rnd.Next(1, 100);
                SpotlightItems = (await _service.GetTopArtistsAsync(page, 10)).Content.ToList();
            }
            catch (Exception e)
            {
                ShowNetworkError(e);
            }
            finally
            {
                IsSliderLoading = false;
            }

            try
            {
                var page = rnd.Next(1, 100);
                TopTracks = (await _service.GetTopTracksAsync(page, 10)).Content.ToList();
            }
            catch (Exception e)
            {
                ShowNetworkError(e);
            }
            finally
            {
                IsFeaturedLoading = false;
            }
            try
            {
                // NewAlbums = (await _service.GetNewAlbums()).Items;
            }
            catch (Exception e)
            {
                ShowNetworkError(e);
            }
            finally
            {
                IsNewLoading = false;
            }
        }

        private void ShowNetworkError(Exception e)
        {
            CurtainToast.ShowError("NetworkIssueToast".FromLanguageResource());

            var ex = e.Message + "\n" + e.StackTrace;
            if (e is LastException)
                ex = (e as LastException).Description + "\n" + ex;
            EasyTracker.GetTracker().SendException(ex, false);
        }
    }
}