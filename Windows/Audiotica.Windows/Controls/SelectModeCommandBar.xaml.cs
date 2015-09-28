using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Audiotica.Core.Exceptions;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Common;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.Views;
using Autofac;

namespace Audiotica.Windows.Controls
{
    public sealed partial class SelectModeCommandBar
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof (ObservableCollection<object>),
                typeof (SelectModeCommandBar), null);

        public SelectModeCommandBar()
        {
            InitializeComponent();

            AppSettings = App.Current.Kernel.Resolve<IAppSettingsUtility>();
        }

        public IAppSettingsUtility AppSettings { get; }

        public bool IsCatalog { get; set; }

        public ObservableCollection<object> SelectedItems
        {
            get { return (ObservableCollection<object>) GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        private IEnumerable<Track> GetTracks()
        {
            List<Track> tracks;

            if (SelectedItems.FirstOrDefault() is Album)
                tracks = SelectedItems.Cast<Album>().SelectMany(p => p.Tracks).ToList();
            else if (SelectedItems.FirstOrDefault() is Artist)
                tracks =
                    SelectedItems.Cast<Artist>()
                        .SelectMany(p => p.Tracks)
                        .Union(SelectedItems.Cast<Artist>().SelectMany(p => p.TracksThatAppearsIn))
                        .ToList();
            else
                tracks = SelectedItems.Cast<Track>().ToList();
            return tracks;
        }

        private async void Play_Click(object sender, RoutedEventArgs e)
        {
            using (var lifetimeScope = App.Current.Kernel.BeginScope())
            {
                var playerService = lifetimeScope.Resolve<IPlayerService>();
                var tracks = GetTracks().Where(p => p.Status == TrackStatus.None || p.Status == TrackStatus.Downloading)
                .ToList();
                await playerService.NewQueueAsync(tracks);
            }
        }

        private async void AddQueue_Click(object sender, RoutedEventArgs e)
        {
            using (var scope = App.Current.Kernel.BeginScope())
            {
                var backgroundAudioService = scope.Resolve<IPlayerService>();
                try
                {
                    var tracks = GetTracks()
                      .Where(p => p.Status == TrackStatus.None || p.Status == TrackStatus.Downloading)
                      .ToList();
                    await backgroundAudioService.AddAsync(tracks);
                    CurtainPrompt.Show("Added to queue");
                }
                catch (AppException ex)
                {
                    CurtainPrompt.ShowError(ex.Message ?? "Something happened.");
                }
            }
        }

        private async void AddUpNext_Click(object sender, RoutedEventArgs e)
        {
            using (var scope = App.Current.Kernel.BeginScope())
            {
                var backgroundAudioService = scope.Resolve<IPlayerService>();
                try
                {
                    var tracks = GetTracks()
                      .Where(p => p.Status == TrackStatus.None || p.Status == TrackStatus.Downloading)
                      .ToList();
                    await backgroundAudioService.AddUpNextAsync(tracks);
                    CurtainPrompt.Show("Added up next");
                }
                catch (AppException ex)
                {
                    CurtainPrompt.ShowError(ex.Message ?? "Something happened.");
                }
            }
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            using (var scope = App.Current.Kernel.BeginScope())
            {
                var downloadService = scope.Resolve<IDownloadService>();
                var tracks = GetTracks().Where(p => p.IsDownloadable);
                foreach (var track in tracks)
                    downloadService.StartDownloadAsync(track);
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            using (var scope = App.Current.Kernel.BeginScope())
            {
                var libraryService = scope.Resolve<ILibraryService>();
                var tracks = GetTracks().ToList();
                foreach (var track in tracks)
                    await libraryService.DeleteTrackAsync(track);

                // make sure to navigate away if album turns out empty
                if (!IsCatalog && App.Current.NavigationService.CurrentPageType == typeof (AlbumPage))
                {
                    if (
                        tracks.Select(
                            track =>
                                libraryService.Albums.FirstOrDefault(p => p.Title.EqualsIgnoreCase(track.AlbumTitle)))
                            .Any(album => album == null))
                    {
                        App.Current.NavigationService.GoBack();
                    }
                }
            }
        }
    }
}