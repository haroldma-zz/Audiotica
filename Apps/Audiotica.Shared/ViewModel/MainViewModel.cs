#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Audiotica.Core.Exceptions;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Service.Interfaces;
using GalaSoft.MvvmLight;
using GoogleAnalytics;
using Microsoft.Xbox.Music.Platform.Contract.DataModel;

#endregion

namespace Audiotica.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IXboxMusicService _service;
        private readonly AudioPlayerManager _audioPlayer;
        private List<XboxAlbum> _featuredReleases;
        private bool _isFeaturedLoading;
        private bool _isNewLoading;
        private bool _isSliderLoading;
        private List<XboxAlbum> _newAlbums;
        private List<XboxArtist> _spotlightItems;

        /// <summary>
        ///     Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IXboxMusicService service, AudioPlayerManager audioPlayer)
        {
            _service = service;
            _audioPlayer = audioPlayer;

            //Load data automatically
            LoadChartDataAsync();
        }

        public List<XboxAlbum> NewAlbums
        {
            get { return _newAlbums; }
            set { Set(ref _newAlbums, value); }
        }

        public List<XboxAlbum> FeatureAlbums
        {
            get { return _featuredReleases; }
            set { Set(ref _featuredReleases, value); }
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

        public List<XboxArtist> SpotlightItems
        {
            get { return _spotlightItems; }
            set { Set(ref _spotlightItems, value); }
        }

        public AudioPlayerManager AudioPlayer
        {
            get { return _audioPlayer; }
        }

        public async Task LoadChartDataAsync()
        {
            IsSliderLoading = true;
            IsFeaturedLoading = true;
            IsNewLoading = true;

            try
            {
                SpotlightItems = (await _service.GetFeaturedArtist(5)).Items;
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
                FeatureAlbums = (await _service.GetFeaturedAlbums()).Items;
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
                NewAlbums = (await _service.GetNewAlbums()).Items;
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
            new MessageDialog("not really... probably a network issue", "oops, some nasty SHIT happened.").ShowAsync();

            var ex = e.Message + "\n" + e.StackTrace;
            if (e is XboxException)
                ex = (e as XboxException).Description + "\n" + ex;
            EasyTracker.GetTracker().SendException(ex, false);
        }
    }
}