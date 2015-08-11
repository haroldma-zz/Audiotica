using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Extensions;
using Audiotica.Core.Windows.Helpers;
using Audiotica.Web.Exceptions;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Metadata.Providers;
using Audiotica.Web.Models;
using Audiotica.Windows.Services;
using Audiotica.Windows.Services.NavigationService;
using Audiotica.Windows.Tools.Mvvm;
using Audiotica.Windows.Views;

namespace Audiotica.Windows.ViewModels
{
    internal sealed class ExplorePageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IWindowsPlayerService _windowsPlayerService;
        private List<WebAlbum> _topAlbums;
        private List<WebArtist> _topArtists;
        private List<WebSong> _topSongs;

        public ExplorePageViewModel(INavigationService navigationService,
            IEnumerable<IMetadataProvider> metadataProviders,
            IWindowsPlayerService windowsPlayerService)
        {
            _navigationService = navigationService;
            _windowsPlayerService = windowsPlayerService;
            MetadataProviders = metadataProviders.Where(p => p.IsEnabled)
                .OrderByDescending(p => p.Priority).ToList();

            SongClickCommand = new Command<ItemClickEventArgs>(SongClickExecute);
            ArtistClickCommand = new Command<ItemClickEventArgs>(ArtistClickExecute);

            if (IsInDesignMode)
                OnNavigatedTo(null, NavigationMode.New, new Dictionary<string, object>());
        }

        public Command<ItemClickEventArgs> ArtistClickCommand { get; set; }
        public Command<ItemClickEventArgs> SongClickCommand { get; }
        public List<IMetadataProvider> MetadataProviders { get; }

        public List<WebSong> TopSongs
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
            _navigationService.Navigate(typeof (ArtistPage),
                new[] {artist.MetadataProvider.AssemblyQualifiedName, artist.Token, artist.Name}.Tokenize());
        }

        private void SongClickExecute(ItemClickEventArgs e)
        {
            var song = (WebSong) e.ClickedItem;
            _windowsPlayerService.Play(song);
        }

        public override void OnNavigatedTo(object parameter, NavigationMode mode, Dictionary<string, object> state)
        {
            var count = DeviceHelper.IsType(DeviceHelper.Family.Mobile) ? 6 : 40;
            LoadTopSongs(count);
            LoadTopArtists(count);
            LoadTopAlbums(count);
        }

        private async void LoadTopSongs(int count)
        {
            foreach (var metadataProvider in MetadataProviders)
            {
                try
                {
                    var results = await metadataProvider.GetTopSongsAsync(count);
                    TopSongs = results.Songs;
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
            foreach (var metadataProvider in MetadataProviders)
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
            foreach (var metadataProvider in MetadataProviders)
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