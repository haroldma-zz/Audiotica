#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Audiotica.Core.Exceptions;
using Audiotica.Data.Model;
using Audiotica.Data.Service.Interfaces;
using GalaSoft.MvvmLight;
using Microsoft.Xbox.Music.Platform.Contract.DataModel;

#endregion

namespace Audiotica.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IXboxMusicService _service;
        private List<XboxAlbum> _featuredReleases;
        private List<XboxAlbum> _newAlbums;
        private List<XboxItem> _spotlightItems;
        private bool _isFeaturedLoading;
        private bool _isSliderLoading;
        private bool _isNewLoading;

        /// <summary>
        ///     Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IXboxMusicService service)
        {
            _service = service;

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

        public List<XboxItem> SpotlightItems
        {
            get { return _spotlightItems; }
            set { Set(ref _spotlightItems, value); }
        }

        public async Task LoadChartDataAsync()
        {
            //IsSliderLoading = true;
            IsFeaturedLoading = true;
            IsNewLoading = true;

            //SpotlightItems = await _service.GetSpotlight();
            //IsSliderLoading = false;

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
            GoogleAnalytics.EasyTracker.GetTracker().SendException(ex, false);
        }
    }
}