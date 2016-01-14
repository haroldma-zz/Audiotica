using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Common;
using Audiotica.Core.Exceptions;
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
using Audiotica.Windows.Extensions;
using Audiotica.Windows.Services.Interfaces;
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
        private readonly IPlayerService _playerService;
        private readonly ISettingsUtility _settingsUtility;
        private readonly ITrackSaveService _trackSaveService;
        private readonly IConverter<WebAlbum, Album> _webAlbumConverter;
        private Album _album;
        private SolidColorBrush _backgroundBrush;

        private Color? _defaultStatusBarForeground;
        private bool _isCatalogMode;
        private ElementTheme _requestedTheme;

        public AlbumPageViewModel(ILibraryService libraryService, INavigationService navigationService,
            IEnumerable<IMetadataProvider> metadataProviders, IConverter<WebAlbum, Album> webAlbumConverter,
            ISettingsUtility settingsUtility, IPlayerService playerService, ITrackSaveService trackSaveService)
        {
            _libraryService = libraryService;
            _navigationService = navigationService;
            _webAlbumConverter = webAlbumConverter;
            _settingsUtility = settingsUtility;
            _playerService = playerService;
            _trackSaveService = trackSaveService;
            _metadataProviders = metadataProviders.FilterAndSort<IExtendedMetadataProvider>();

            ViewInCatalogCommand = new Command(ViewInCatalogExecute);
            PlayAllCommand = new Command(PlayAllExecute);
            SaveAllCommand = new Command<object>(SaveAllExecute);

            if (IsInDesignMode)
                OnNavigatedTo(new AlbumPageParameter("Kauai", "Childish Gambino"), NavigationMode.New,
                    new Dictionary<string, object>());
        }

        public Command<object> SaveAllCommand { get; }

        public Command PlayAllCommand { get; }

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

        private async void SaveAllExecute(object sender)
        {
            foreach (var track in Album.Tracks.Where(p => !p.IsFromLibrary))
            {
                try
                {
                    await _trackSaveService.SaveAsync(track);
                }
                catch (AppException ex)
                {
                    track.Status = TrackStatus.None;
                    CurtainPrompt.ShowError(ex.Message ?? "Problem saving: " + track);
                }
            }
        }

        private async void PlayAllExecute()
        {
            if (Album.Tracks.Count == 0) return;
            var albumTracks = Album.Tracks.ToList();
            await _playerService.NewQueueAsync(albumTracks);
        }

        private void ViewInCatalogExecute()
        {
            _navigationService.GoBack();
            _navigationService.Navigate(typeof (AlbumPage),
                new AlbumPageParameter(Album.Title, Album.Artist.Name) {IsCatalogMode = true});
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
                    await LoadAsync(albumParameter);
                }
                catch
                {
                    // ignored
                }

                if (Album == null)
                {
                    CurtainPrompt.ShowError("Problem loading album");
                    return;
                }
            }

            if (DeviceHelper.IsType(DeviceFamily.Mobile))
                _defaultStatusBarForeground = StatusBar.GetForCurrentView().ForegroundColor;
            if (_settingsUtility.Read(ApplicationSettingsConstants.IsAlbumAdaptiveColorEnabled, true))
                DetectColorFromArtwork();
        }

        private async Task LoadAsync(AlbumPageParameter albumParameter)
        {
            var webAlbum = albumParameter.WebAlbum;
            if (webAlbum == null && albumParameter.Provider != null)
            {
                var provider = _metadataProviders.FirstOrDefault(p => p.GetType() == albumParameter.Provider);
                webAlbum = await provider.GetAlbumAsync(albumParameter.Token);
            }
            if (webAlbum != null)
            {
                try
                {
                    Album = await _webAlbumConverter.ConvertAsync(webAlbum, IsCatalogMode);
                }
                catch
                {
                    await LoadByTitleAsync(albumParameter);
                }
            }
            else
                await LoadByTitleAsync(albumParameter);
        }

        private async Task LoadByTitleAsync(AlbumPageParameter albumParameter)
        {
            for (var i = 0; i < _metadataProviders.Count; i++)
            {
                try
                {
                    var webAlbum = await GetAlbumByTitleAsync(albumParameter.Title, albumParameter.Artist, i);

                    if (webAlbum != null)
                    {
                        Album = await _webAlbumConverter.ConvertAsync(webAlbum, IsCatalogMode);
                        break;
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        public override void OnNavigatedFrom()
        {
            // Bug: if we don't reset the theme when we go out it fucks with the TrackViewer control on other pages
            RequestedTheme = ElementTheme.Default;
            if (DeviceHelper.IsType(DeviceFamily.Mobile))
                StatusBar.GetForCurrentView().ForegroundColor = _defaultStatusBarForeground;
        }

        private async Task<WebAlbum> GetAlbumByTitleAsync(string title, string artist, int providerIndex)
        {
            try
            {
                var webAlbum = await _metadataProviders[providerIndex].GetAlbumByTitleAsync(title, artist);
                if (webAlbum != null) return webAlbum;
            }
            catch
            {
                // ignored
            }
            return null;
        }

        private async void DetectColorFromArtwork()
        {
            if (string.IsNullOrWhiteSpace(Album.ArtworkUri)) return;

            try
            {
                using (var stream = await Album.ArtworkUri.ToUri().GetStreamAsync())
                {
                    var main = ColorThief.GetColor(await stream.ToWriteableBitmapAsync());

                    BackgroundBrush = new SolidColorBrush(main.Color);
                    RequestedTheme = main.IsDark ? ElementTheme.Dark : ElementTheme.Light;
                    if (DeviceHelper.IsType(DeviceFamily.Mobile))
                        StatusBar.GetForCurrentView().ForegroundColor =
                            (main.IsDark ? Colors.White : Colors.Black) as Color?;
                }
            }
            catch
            {
                // ignored
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