using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Core.Windows.Helpers;
using Audiotica.Database.Models;
using Audiotica.Web.Extensions;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Windows.Controls;
using Audiotica.Windows.Engine;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.ViewModels;
using Microsoft.AdMediator.Universal;

namespace Audiotica.Windows
{
    public sealed partial class Shell : INotifyPropertyChanged
    {
        private CancellationTokenSource _cancellationTokenSource;
        private string _currentSongSlug;
        private bool _flyoutOpened;
        private bool _isLyricsLoading;
        private string _lyricsText;

        public Shell()
        {
            Instance = this;
            InitializeComponent();
            Loaded += Shell_Loaded;
            ViewModel = App.Current.Kernel.Resolve<PlayerBarViewModel>();
            AppSettings = App.Current.Kernel.Resolve<IAppSettingsUtility>();
            RequestedTheme = (ElementTheme)AppSettings.Theme;
            HamburgerMenu.RefreshStyles((ElementTheme)AppSettings.Theme);
            AppSettings.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(AppSettings.Theme))
                    {
                        RequestedTheme = (ElementTheme)AppSettings.Theme;
                        HamburgerMenu.RefreshStyles((ElementTheme)AppSettings.Theme);
                    }
                };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static HamburgerMenu HamburgerMenu => Instance.BurgerMenu;

        public static Shell Instance { get; set; }

        public bool AdsLoaded { get; private set; }

        public IAppSettingsUtility AppSettings { get; }

        public string BusyText { get; set; } = "Please wait...";

        public bool IsBusy { get; set; }

        public bool IsLyricsLoading
        {
            get
            {
                return _isLyricsLoading;
            }
            set
            {
                _isLyricsLoading = value;
                OnPropertyChanged();
            }
        }

        public string LyricsText
        {
            get
            {
                return _lyricsText;
            }
            set
            {
                _lyricsText = value;
                OnPropertyChanged();
            }
        }

        public PlayerBarViewModel ViewModel { get; }

        public static void SetBusy(bool busy, string text = null)
        {
            WindowWrapper.Current().Dispatcher.Run(() =>
                {
                    if (busy)
                    {
                        SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                            AppViewBackButtonVisibility.Collapsed;
                    }
                    else
                    {
                        BootStrapper.Current.UpdateShellBackButton();
                    }

                    Instance.IsBusy = busy;
                    Instance.BusyText = text;

                    Instance.PropertyChanged?.Invoke(Instance, new PropertyChangedEventArgs(nameof(IsBusy)));
                    Instance.PropertyChanged?.Invoke(Instance, new PropertyChangedEventArgs(nameof(BusyText)));
                });
        }

        public void ConfigureAds()
        {
            /*
               Windows Desktop     Windows Phone & Windows Mobile
               250×250*            300×50
               300×250             320×50
               728×90              480×80
               160×600             640×100
               300×600	
               */

            var mediatorBar = new AdMediatorControl
            {
                Id = "AdMediator-Id-13B224DA-AEC5-41E6-9B0A-FE01E4E1EB2B",
                Name = "AdMediator_3CB848",
                Width = 728,
                Height = 90
            };
            var row = 2;
            if (DeviceHelper.IsType(DeviceFamily.Mobile))
            {
                row = 3;
                mediatorBar.Width = 320;
                mediatorBar.Height = 50;
            }

            Grid.SetRow(mediatorBar, row);
            RootLayout.Children.Add(mediatorBar);
            AdsLoaded = true;
        }

        public void DisableAds()
        {
            var mediatorBar = RootLayout.Children[RootLayout.Children.Count - 1] as AdMediatorControl;
            mediatorBar.Dispose();
            RootLayout.Children.Remove(mediatorBar);
            AdsLoaded = false;
        }

        private async void LyricsFlyout_OnOpened(object sender, object e)
        {
            _flyoutOpened = true;
            var track = ViewModel.CurrentQueueTrack.Track;
            var newSlug = TrackComparer.GetSlug(track);
            if (_currentSongSlug == newSlug)
            {
                return;
            }
            _currentSongSlug = newSlug;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            IsLyricsLoading = true;
            LyricsText = "Loading lyrics...";

            if (!string.IsNullOrEmpty(track.Lyrics))
            {
                LyricsText = track.Lyrics;
                IsLyricsLoading = false;
                return;
            }

            var providers = App.Current.Kernel.Resolve<IMetadataProvider[]>().FilterAndSort<ILyricsMetadataProvider>();

            try
            {
                foreach (var lyricsMetadataProvider in providers)
                {
                    var lyrics = await lyricsMetadataProvider.GetLyricAsync(track.Title, track.AlbumArtist);
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    if (string.IsNullOrEmpty(lyrics))
                    {
                        lyrics = await lyricsMetadataProvider.GetLyricAsync(track.Title, track.DisplayArtist);
                    }
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    if (string.IsNullOrEmpty(lyrics))
                    {
                        continue;
                    }

                    LyricsText = lyrics;
                    break;
                }

                if (LyricsText == "Loading lyrics...")
                {
                    LyricsText = "No lyrics found.";
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch
            {
                LyricsText = "Something happened :/";
            }
            finally
            {
                IsLyricsLoading = false;
            }
        }

        private void LyricsFlyoutBase_OnClosed(object sender, object e) => _flyoutOpened = false;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PlayerServiceOnTrackChanged(object sender, string s)
        {
            if (IsLyricsLoading || _flyoutOpened)
            {
                LyricsFlyout_OnOpened(null, null);
            }
        }

        private void Shell_Loaded(object sender, RoutedEventArgs e)
        {
            HamburgerMenu.NavigationService = App.Current.NavigationService;

            if (AppSettings.Ads)
            {
                ConfigureAds();
            }
            var playerService = App.Current.Kernel.Resolve<IPlayerService>();
            playerService.TrackChanged += PlayerServiceOnTrackChanged;
        }
    }
}