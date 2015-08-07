using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Common;
using Audiotica.Core.Windows.Helpers;
using Audiotica.Core.Windows.Services;
using Audiotica.Database.Models;
using Audiotica.Factory;
using Audiotica.Web.Exceptions;
using Audiotica.Web.MatchEngine.Interfaces.Providers;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;
using Audiotica.Windows.Common;
using Audiotica.Windows.Services;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    internal sealed class ExplorePageViewModel : ViewModelBase
    {
        private readonly IWindowsPlayerService _windowsPlayerService;
        private List<WebAlbum> _topAlbums;
        private List<WebArtist> _topArtists;
        private List<WebSong> _topSongs;

        public ExplorePageViewModel(IEnumerable<IMetadataProvider> metadataProviders, IWindowsPlayerService windowsPlayerService)
        {
            _windowsPlayerService = windowsPlayerService;
            MetadataProviders = metadataProviders.Where(p => p.IsEnabled)
                .OrderByDescending(p => p.Priority).ToList();

            SongClickCommand = new Command<ItemClickEventArgs>(SongClickExecute);

            if (IsInDesignMode)
                OnNavigatedTo(null, NavigationMode.New, new Dictionary<string, object>());
        }

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

        private void SongClickExecute(ItemClickEventArgs e)
        {
            var song = (WebSong) e.ClickedItem;
            _windowsPlayerService.Play(song);
        }

        public override void OnNavigatedTo(string parameter, NavigationMode mode, Dictionary<string, object> state)
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