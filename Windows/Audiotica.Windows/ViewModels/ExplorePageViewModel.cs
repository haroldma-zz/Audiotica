using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Common;
using Audiotica.Core.Exceptions;
using Audiotica.Core.Windows.Helpers;
using Audiotica.Database.Models;
using Audiotica.Web.Exceptions;
using Audiotica.Web.Extensions;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;
using Audiotica.Windows.Common;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.Services.NavigationService;
using Audiotica.Windows.Tools.Mvvm;
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

        public ExplorePageViewModel(INavigationService navigationService,
            IEnumerable<IMetadataProvider> metadataProviders,
            IPlayerService playerService, IConverter<WebSong, Track> webToTrackConverter)
        {
            _navigationService = navigationService;
            _playerService = playerService;
            _webToTrackConverter = webToTrackConverter;
            _metadataProviders = metadataProviders.FilterAndSort<IChartMetadataProvider>();
            
            ArtistClickCommand = new Command<ItemClickEventArgs>(ArtistClickExecute);
            AlbumClickCommand = new Command<ItemClickEventArgs>(AlbumClickExecute);

            if (IsInDesignMode)
                OnNavigatedTo(null, NavigationMode.New, new Dictionary<string, object>());
        }

        private void AlbumClickExecute(ItemClickEventArgs e)
        {
            var album = (WebAlbum)e.ClickedItem;
            _navigationService.Navigate(typeof(AlbumPage),
                new AlbumPageViewModel.AlbumPageParameter(album.Title, album.Artist.Name, album) { IsCatalogMode = true });
        }

        public Command<ItemClickEventArgs> AlbumClickCommand { get; set; }

        public Command<ItemClickEventArgs> ArtistClickCommand { get; set; }

        public List<Track> TopSongs
        {
            get { return _topSongs; }
            set { Set(ref _topSongs, value); }
        }

        public List<WebAlbum> TopAlbums
        {
            get { return _topAlbums; }
            set { Set(ref _topAlbums, value); }
        }

        public List<WebArtist> TopArtists
        {
            get { return _topArtists; }
            set { Set(ref _topArtists, value); }
        }

        private void ArtistClickExecute(ItemClickEventArgs e)
        {
            var artist = (WebArtist) e.ClickedItem;
            _navigationService.Navigate(typeof (ArtistPage), artist.Name);
        }

        public override void OnNavigatedTo(object parameter, NavigationMode mode, Dictionary<string, object> state)
        {
            var count = DeviceHelper.IsType(DeviceFamily.Mobile) ? 6 : 20;
            LoadTopSongs(count);
            LoadTopArtists(count);
            LoadTopAlbums(count);
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
    }
}