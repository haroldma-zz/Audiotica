#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
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
        private List<XboxArtist> _featuredSlider;
        private List<XboxAlbum> _newAlbums;
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

        public List<XboxArtist> FeaturedSliderArtists
        {
            get { return _featuredSlider; }
            set { Set(ref _featuredSlider, value); }
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

        public async Task LoadChartDataAsync()
        {
            try
            {
                IsSliderLoading = true;
                IsFeaturedLoading = true;
                IsNewLoading = true;

                FeaturedSliderArtists = (await _service.GetFeaturedArtist()).Items;
                IsSliderLoading = false;

                FeatureAlbums = (await _service.GetFeaturedAlbums()).Items;
                IsFeaturedLoading = false;

                NewAlbums = (await _service.GetNewAlbums()).Items;
                IsNewLoading = false;
            }
            catch (Exception e)
            {
                Debugger.Break();
            }
        }
    }
}