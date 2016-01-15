using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Common;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Core.Windows.Extensions;
using Audiotica.Core.Windows.Helpers;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Web.Extensions;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;
using Audiotica.Windows.Common;
using Audiotica.Windows.Engine.Mvvm;
using Audiotica.Windows.Engine.Navigation;
using Audiotica.Windows.Extensions;
using Audiotica.Windows.Views;

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
        private Color? _defaultStatusBarForeground;
        private SolidColorBrush _foregroundBrush;
        private bool _isAlbumsLoading;
        private bool _isNewAlbumsLoading;
        private bool _isTopSongsLoading;
        private List<WebAlbum> _newAlbums;
        private ElementTheme _requestedTheme = ElementTheme.Default;
        private List<WebAlbum> _topAlbums;
        private List<Track> _topSongs;

        public ArtistPageViewModel(
            INavigationService navigationService,
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

            AlbumClickCommand = new DelegateCommand<ItemClickEventArgs>(AlbumClickExecute);
            WebAlbumClickCommand = new DelegateCommand<ItemClickEventArgs>(WebAlbumClickExecute);

            if (IsInDesignMode)
            {
                OnNavigatedTo("Childish Gambino", NavigationMode.New, new Dictionary<string, object>());
            }
        }

        public DelegateCommand<ItemClickEventArgs> AlbumClickCommand { get; set; }

        public Artist Artist
        {
            get
            {
                return _artist;
            }
            set
            {
                Set(ref _artist, value);
            }
        }

        public SolidColorBrush BackgroundBrush
        {
            get
            {
                return _backgroundBrush;
            }
            set
            {
                Set(ref _backgroundBrush, value);
            }
        }

        public SolidColorBrush ForegroundBrush
        {
            get
            {
                return _foregroundBrush;
            }
            set
            {
                Set(ref _foregroundBrush, value);
            }
        }

        public bool IsAlbumsLoading
        {
            get
            {
                return _isAlbumsLoading;
            }
            set
            {
                Set(ref _isAlbumsLoading, value);
            }
        }

        public bool IsNewAlbumsLoading
        {
            get
            {
                return _isNewAlbumsLoading;
            }
            set
            {
                Set(ref _isNewAlbumsLoading, value);
            }
        }

        public bool IsTopSongsLoading
        {
            get
            {
                return _isTopSongsLoading;
            }
            set
            {
                Set(ref _isTopSongsLoading, value);
            }
        }

        public List<WebAlbum> NewAlbums
        {
            get
            {
                return _newAlbums;
            }
            set
            {
                Set(ref _newAlbums, value);
            }
        }

        public ElementTheme RequestedTheme
        {
            get
            {
                return _requestedTheme;
            }
            set
            {
                Set(ref _requestedTheme, value);
            }
        }

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

        public DelegateCommand<ItemClickEventArgs> WebAlbumClickCommand { get; }

        public override async void OnNavigatedTo(
            object parameter,
            NavigationMode mode,
            IDictionary<string, object> state)
        {
            var name = parameter as string;

            Artist = _libraryService.Artists.FirstOrDefault(p => p.Name.EqualsIgnoreCase(name));
            if (Artist?.ArtworkUri == null)
            {
                foreach (var provider in _metadataProviders)
                {
                    try
                    {
                        var webArtist = await provider.GetArtistByNameAsync(name);

                        if (Artist != null && Artist.ArtworkUri == null)
                        {
                            Artist.ArtworkUri = webArtist.Artwork.ToString();
                        }
                        else
                        {
                            Artist = await _webArtistConverter.ConvertAsync(webArtist);
                        }
                        if (Artist != null)
                        {
                            break;
                        }
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

            if (DeviceHelper.IsType(DeviceFamily.Mobile))
            {
                _defaultStatusBarForeground = StatusBar.GetForCurrentView().ForegroundColor;
            }
            if (_settingsUtility.Read(ApplicationSettingsConstants.IsArtistAdaptiveColorEnabled, true))
            {
                DetectColorFromArtwork();
            }
            LoadWebData();
        }

        public override void OnNavigatingFrom(NavigatingEventArgs args)
        {
            // Bug: if we don't reset the theme when we go out it fucks with the TrackViewer control on other pages
            RequestedTheme = ElementTheme.Default;
            if (DeviceHelper.IsType(DeviceFamily.Mobile))
            {
                StatusBar.GetForCurrentView().ForegroundColor = _defaultStatusBarForeground;
            }
        }

        private void AlbumClickExecute(ItemClickEventArgs e)
        {
            var album = (Album)e.ClickedItem;
            _navigationService.Navigate(typeof (AlbumPage),
                new AlbumPageViewModel.AlbumPageParameter(album.Title, album.Artist.Name));
        }

        private async void DetectColorFromArtwork()
        {
            if (string.IsNullOrWhiteSpace(Artist.ArtworkUri))
            {
                return;
            }

            try
            {
                using (var stream = await Artist.ArtworkUri.ToUri().GetStreamAsync())
                {
                    var main = ColorThief.GetColor(await stream.ToWriteableBitmapAsync());

                    BackgroundBrush = new SolidColorBrush(main.Color);
                    RequestedTheme = main.IsDark ? ElementTheme.Dark : ElementTheme.Light;
                    if (DeviceHelper.IsType(DeviceFamily.Mobile))
                    {
                        StatusBar.GetForCurrentView().ForegroundColor =
                            main.IsDark ? Colors.White : Colors.Black;
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        private async void LoadWebData()
        {
            IsNewAlbumsLoading = true;
            IsAlbumsLoading = true;
            IsTopSongsLoading = true;

            foreach (var metadataProvider in _metadataProviders)
            {
                try
                {
                    var webArtist = await metadataProvider.GetArtistByNameAsync(Artist.Name);
                    if (webArtist == null)
                    {
                        continue;
                    }

                    try
                    {
                        // since it will remove duplicates request 4
                        if (NewAlbums == null)
                        {
                            NewAlbums =
                                await _webAlbumConverter.FillPartialAsync(
                                    (await metadataProvider.GetArtistNewAlbumsAsync(webArtist.Token, 4)).Albums.Take(2));
                            IsNewAlbumsLoading = false;
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    try
                    {
                        if (TopAlbums == null)
                        {
                            TopAlbums =
                                await
                                    _webAlbumConverter.FillPartialAsync(
                                        (await metadataProvider.GetArtistAlbumsAsync(webArtist.Token, 10)).Albums);
                            IsAlbumsLoading = false;
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    try
                    {
                        if (TopSongs == null)
                        {
                            TopSongs = await _webSongConverter.ConvertAsync(
                                (await metadataProvider.GetArtistTopSongsAsync(webArtist.Token, 5)).Songs);
                            IsTopSongsLoading = false;
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    if (TopSongs != null && TopAlbums != null && NewAlbums != null)
                    {
                        break;
                    }
                }
                catch
                {
                    // ignored
                }
            }

            IsNewAlbumsLoading = false;
            IsAlbumsLoading = false;
            IsTopSongsLoading = false;
        }

        private void WebAlbumClickExecute(ItemClickEventArgs e)
        {
            var album = (WebAlbum)e.ClickedItem;
            _navigationService.Navigate(typeof (AlbumPage),
                new AlbumPageViewModel.AlbumPageParameter(album.Title, album.Artist.Name, album)
                {
                    IsCatalogMode = true
                });
        }
    }
}