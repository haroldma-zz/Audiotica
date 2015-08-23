using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Common;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Core.Windows.Extensions;
using Audiotica.Core.Windows.Helpers;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Web.Exceptions;
using Audiotica.Web.Extensions;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;
using Audiotica.Windows.Common;
using Audiotica.Windows.Services.NavigationService;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    public sealed class ArtistPageViewModel : ViewModelBase
    {
        private readonly ILibraryService _libraryService;
        private readonly List<IExtendedMetadataProvider> _metadataProviders;
        private readonly INavigationService _navigationService;
        private readonly ISettingsUtility _settingsUtility;
        private readonly IConverter<WebAlbum, Album> _webAlbumConverter;
        private readonly IConverter<WebArtist, Artist> _webArtistConverter;
        private readonly IConverter<WebSong, Track> _webSongConverter;
        private Artist _artist;
        private SolidColorBrush _backgroundBrush;
        private SolidColorBrush _foregroundBrush;
        private ElementTheme _requestedTheme = ElementTheme.Light;
        private List<Album> _topAlbums;
        private List<Track> _topSongs;

        public ArtistPageViewModel(INavigationService navigationService,
            ILibraryService libraryService,
            IEnumerable<IMetadataProvider> metadataProviders,
            IConverter<WebAlbum, Album> webAlbumConverter,
            IConverter<WebArtist, Artist> webArtistConverter,
            IConverter<WebSong, Track> webSongConverter,
            ISettingsUtility settingsUtility)
        {
            _navigationService = navigationService;
            _libraryService = libraryService;
            _webAlbumConverter = webAlbumConverter;
            _metadataProviders = metadataProviders.FilterAndSort<IExtendedMetadataProvider>();

            _webArtistConverter = webArtistConverter;
            _webSongConverter = webSongConverter;
            _settingsUtility = settingsUtility;

            if (IsInDesignMode)
                OnNavigatedTo("Childish Gambino", NavigationMode.New, new Dictionary<string, object>());
        }

        public ElementTheme RequestedTheme
        {
            get { return _requestedTheme; }
            set { Set(ref _requestedTheme, value); }
        }

        public SolidColorBrush ForegroundBrush
        {
            get { return _foregroundBrush; }
            set { Set(ref _foregroundBrush, value); }
        }

        public SolidColorBrush BackgroundBrush
        {
            get { return _backgroundBrush; }
            set { Set(ref _backgroundBrush, value); }
        }

        public Artist Artist
        {
            get { return _artist; }
            set { Set(ref _artist, value); }
        }

        public List<Album> TopAlbums
        {
            get { return _topAlbums; }
            set { Set(ref _topAlbums, value); }
        }

        public List<Track> TopSongs
        {
            get { return _topSongs; }
            set { Set(ref _topSongs, value); }
        }

        public override async void OnNavigatedTo(object parameter, NavigationMode mode, Dictionary<string, object> state)
        {
            var name = parameter as string;

            Artist = _libraryService.Artists.FirstOrDefault(p => p.Name == name);
            if (Artist == null)
            {
                foreach (var provider in _metadataProviders)
                {
                    try
                    {
                        var webArtist = await provider.GetArtistByNameAsync(name);
                        Artist = await _webArtistConverter.ConvertAsync(webArtist);
                        if (Artist != null) break;
                    }
                    catch
                    {
                        // ignored
                    }
                }

                if (Artist == null)
                {
                    _navigationService.GoBack();
                    return;
                }
            }


            if (_settingsUtility.Read(ApplicationSettingsConstants.IsArtistAdaptiveColorEnabled, true))
                DetectColorFromArtwork();
            LoadWebData();
        }

        private async void DetectColorFromArtwork()
        {
            using (var response = await Artist.ArtworkUri.ToUri().GetAsync())
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    var main = ColorThief.GetColor(await stream.ToWriteableBitmapAsync());

                    BackgroundBrush = new SolidColorBrush(main.Color);
                    RequestedTheme = main.IsDark ? ElementTheme.Dark : ElementTheme.Light;
                }
            }
        }

        private async void LoadWebData()
        {
            foreach (var metadataProvider in _metadataProviders)
            {
                try
                {
                    var webArtist = await metadataProvider.GetArtistByNameAsync(Artist.Name);
                    if (webArtist == null) continue;

                    if (TopSongs == null)
                        TopSongs =
                            await
                                _webSongConverter.ConvertAsync(
                                    (await metadataProvider.GetArtistTopSongsAsync(webArtist.Token, 5)).Songs);

                    if (TopAlbums == null)
                        TopAlbums =
                            await
                                _webAlbumConverter.ConvertAsync(
                                    (await metadataProvider.GetArtistAlbumsAsync(webArtist.Token, 10)).Albums);

                    if (TopSongs != null && TopAlbums != null) break;
                }
                catch (NotImplementedException)
                {
                }
                catch (ProviderException)
                {
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}