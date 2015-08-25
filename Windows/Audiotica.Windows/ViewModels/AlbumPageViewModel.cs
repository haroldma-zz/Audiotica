using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Audiotica.Web.Extensions;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;
using Audiotica.Windows.Common;
using Audiotica.Windows.Services.NavigationService;
using Audiotica.Windows.Tools.Mvvm;
using Audiotica.Windows.Views;

namespace Audiotica.Windows.ViewModels
{
    public class AlbumPageViewModel : ViewModelBase
    {
        private readonly ILibraryService _libraryService;
        private readonly List<IExtendedMetadataProvider> _metadataProviders;
        private readonly INavigationService _navigationService;
        private readonly ISettingsUtility _settingsUtility;
        private readonly IConverter<WebAlbum, Album> _webAlbumConverter;
        private Album _album;
        private SolidColorBrush _backgroundBrush;
        private bool _isCatalogMode;
        private ElementTheme _requestedTheme;

        public AlbumPageViewModel(ILibraryService libraryService, INavigationService navigationService,
            IEnumerable<IMetadataProvider> metadataProviders, IConverter<WebAlbum, Album> webAlbumConverter,
            ISettingsUtility settingsUtility)
        {
            _libraryService = libraryService;
            _navigationService = navigationService;
            _webAlbumConverter = webAlbumConverter;
            _settingsUtility = settingsUtility;
            _metadataProviders = metadataProviders.FilterAndSort<IExtendedMetadataProvider>();

            ViewInCatalogCommand = new Command(ViewInCatalogExecute);

            if (IsInDesignMode)
                OnNavigatedTo(new AlbumPageParameter("Kauai", "Childish Gambino"), NavigationMode.New,
                    new Dictionary<string, object>());
        }

        public Command ViewInCatalogCommand { get; }

        public Album Album
        {
            get { return _album; }
            set { Set(ref _album, value); }
        }

        public ElementTheme RequestedTheme
        {
            get { return _requestedTheme; }
            set { Set(ref _requestedTheme, value); }
        }

        public SolidColorBrush BackgroundBrush
        {
            get { return _backgroundBrush; }
            set { Set(ref _backgroundBrush, value); }
        }

        public bool IsCatalogMode
        {
            get { return _isCatalogMode; }
            set { Set(ref _isCatalogMode, value); }
        }

        private void ViewInCatalogExecute()
        {
            _navigationService.GoBack();
            _navigationService.Navigate(typeof (AlbumPage), new AlbumPageParameter(Album.Title, Album.Artist.Name) {IsCatalogMode = true});
        }

        public override sealed async void OnNavigatedTo(object parameter, NavigationMode mode,
            Dictionary<string, object> state)
        {
            var albumParameter = (AlbumPageParameter) parameter;
            IsCatalogMode = albumParameter.IsCatalogMode;

            if (!IsCatalogMode && albumParameter.Title != null)
                Album = _libraryService.Albums.FirstOrDefault(p =>
                    p.Title.EqualsIgnoreCase(albumParameter.Title) &&
                    p.Artist.Name.EqualsIgnoreCase(albumParameter.Artist));
            if (Album == null)
            {
                try
                {
                    var webAlbum = albumParameter.WebAlbum;

                    if (webAlbum == null)
                    {
                        if (albumParameter.Provider != null)
                        {
                            var provider = _metadataProviders.FirstOrDefault(p => p.GetType() == albumParameter.Provider);
                            webAlbum = await provider.GetAlbumAsync(albumParameter.Token);
                        }
                        else
                        {
                            webAlbum = await GetAlbumByTitleAsync(albumParameter.Title, albumParameter.Artist);
                        }
                    }

                    if (webAlbum != null)
                        Album = await _webAlbumConverter.ConvertAsync(webAlbum, IsCatalogMode);
                }
                catch
                {
                    // ignored
                }

                if (Album == null)
                {
                    _navigationService.GoBack();
                }
            }

            if (_settingsUtility.Read(ApplicationSettingsConstants.IsAlbumAdaptiveColorEnabled, true))
                DetectColorFromArtwork();
        }

        public override void OnNavigatedFrom(bool suspending, Dictionary<string, object> state)
        {
            // Bug: if we don't reset the theme when we go out it fucks with the TrackViewer control on other pages
            RequestedTheme = ElementTheme.Default;
        }

        private async Task<WebAlbum> GetAlbumByTitleAsync(string title, string artist)
        {
            foreach (var provider in _metadataProviders)
            {
                try
                {
                    var webAlbum = await provider.GetAlbumByTitleAsync(title, artist);
                    if (webAlbum != null) return webAlbum;
                }
                catch
                {
                    // ignored
                }
            }
            return null;
        }

        private async void DetectColorFromArtwork()
        {
            if (string.IsNullOrWhiteSpace(Album.ArtworkUri)) return;

            using (var response = await Album.ArtworkUri.ToUri().GetAsync())
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    var main = ColorThief.GetColor(await stream.ToWriteableBitmapAsync());

                    BackgroundBrush = new SolidColorBrush(main.Color);
                    RequestedTheme = main.IsDark ? ElementTheme.Dark : ElementTheme.Light;
                }
            }
        }

        public class AlbumPageParameter
        {
            public AlbumPageParameter()
            {
            }

            public AlbumPageParameter(string token, Type provider)
            {
                Token = token;
                Provider = provider;
            }

            public AlbumPageParameter(string title, string artist, string token, Type provider)
            {
                Title = title;
                Artist = artist;
                Token = token;
                Provider = provider;
            }

            public AlbumPageParameter(string title, string artist, WebAlbum webAlbum)
            {
                Title = title;
                Artist = artist;
                WebAlbum = webAlbum;
            }

            public AlbumPageParameter(string title, string artist)
            {
                Title = title;
                Artist = artist;
            }

            public Type Provider { get; set; }
            public string Token { get; set; }
            public string Title { get; set; }
            public string Artist { get; set; }
            public WebAlbum WebAlbum { get; set; }
            public bool IsCatalogMode { get; set; }
        }
    }
}