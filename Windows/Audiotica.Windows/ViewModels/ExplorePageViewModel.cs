using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Common;
using Audiotica.Core.Windows.Helpers;
using Audiotica.Database.Models;
using Audiotica.Web.Exceptions;
using Audiotica.Web.Extensions;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;
using Audiotica.Windows.Engine.Mvvm;
using Audiotica.Windows.Engine.Navigation;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.Views;

namespace Audiotica.Windows.ViewModels
{
    public sealed class ExplorePageViewModel : ViewModelBase
    {
        private readonly List<IChartMetadataProvider> _metadataProviders;
        private readonly INavigationService _navigationService;
        private readonly IPlayerService _playerService;
        private readonly IConverter<WebSong, Track> _webToTrackConverter;
        private List<WebAlbum> _topAlbums;
        private List<WebArtist> _topArtists;
        private List<Track> _topSongs;

        public ExplorePageViewModel(
            INavigationService navigationService,
            IEnumerable<IMetadataProvider> metadataProviders,
            IPlayerService playerService,
            IConverter<WebSong, Track> webToTrackConverter)
        {
            _navigationService = navigationService;
            _playerService = playerService;
            _webToTrackConverter = webToTrackConverter;
            _metadataProviders = metadataProviders.FilterAndSort<IChartMetadataProvider>();

            ArtistClickCommand = new DelegateCommand<ItemClickEventArgs>(ArtistClickExecute);
            AlbumClickCommand = new DelegateCommand<ItemClickEventArgs>(AlbumClickExecute);

            if (IsInDesignMode)
            {
                OnNavigatedTo(null, NavigationMode.New, new Dictionary<string, object>());
            }
        }

        public DelegateCommand<ItemClickEventArgs> AlbumClickCommand { get; set; }

        public DelegateCommand<ItemClickEventArgs> ArtistClickCommand { get; set; }

        public List<WebAlbum> TopAlbums
        {
            get
            {
                return _topAlbums;
            }
            set
            {
                Set(ref _topAlbums, value);
            }
        }

        public List<WebArtist> TopArtists
        {
            get
            {
                return _topArtists;
            }
            set
            {
                Set(ref _topArtists, value);
            }
        }

        public List<Track> TopSongs
        {
            get
            {
                return _topSongs;
            }
            set
            {
                Set(ref _topSongs, value);
            }
        }

        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            AnalyticService.TrackPageView("Explore");
            var count = DeviceHelper.IsType(DeviceFamily.Mobile) ? 6 : 20;
            LoadTopSongs(count);
            LoadTopArtists(count);
            LoadTopAlbums(count);
        }

        private void AlbumClickExecute(ItemClickEventArgs e)
        {
            var album = (WebAlbum)e.ClickedItem;
            _navigationService.Navigate(typeof(AlbumPage),
                new AlbumPageViewModel.AlbumPageParameter(album.Title, album.Artist.Name, album)
                {
                    IsCatalogMode = true
                });
        }

        private void ArtistClickExecute(ItemClickEventArgs e)
        {
            var artist = (WebArtist)e.ClickedItem;
            _navigationService.Navigate(typeof(ArtistPage), artist.Name);
        }

        private async void LoadTopAlbums(int count)
        {
            foreach (var metadataProvider in _metadataProviders)
            {
                try
                {
                    var results = await metadataProvider.GetTopAlbumsAsync(count);
                    TopAlbums = results.Albums;
                    break;
                }
                catch (NotImplementedException)
                {
                }
                catch (ProviderException)
                {
                }
            }
        }

        private async void LoadTopArtists(int count)
        {
            foreach (var metadataProvider in _metadataProviders)
            {
                try
                {
                    var results = await metadataProvider.GetTopArtistsAsync(count);
                    TopArtists = results.Artists;
                    break;
                }
                catch (NotImplementedException)
                {
                }
                catch (ProviderException)
                {
                }
            }
        }

        private async void LoadTopSongs(int count)
        {
            foreach (var metadataProvider in _metadataProviders)
            {
                try
                {
                    var results = await metadataProvider.GetTopSongsAsync(count);
                    TopSongs = await _webToTrackConverter.ConvertAsync(results.Songs);
                    break;
                }
                catch (NotImplementedException)
                {
                }
                catch (ProviderException)
                {
                }
            }
        }
    }
}