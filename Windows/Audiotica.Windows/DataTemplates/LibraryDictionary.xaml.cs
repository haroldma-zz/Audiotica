using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Audiotica.Core.Exceptions;
using Audiotica.Core.Extensions;
using Audiotica.Core.Windows.Helpers;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Common;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.Views;
using Autofac;

namespace Audiotica.Windows.DataTemplates
{
    public sealed partial class LibraryDictionary
    {
        public LibraryDictionary()
        {
            InitializeComponent();
        }

        private void Panel_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var panel = (Grid) sender;
            FlyoutEx.ShowAttachedFlyoutAtPointer(panel);
        }

        private static IEnumerable<Track> GetTracks(object item)
        {
            List<Track> tracks;

            if (item is Album)
                tracks = item.As<Album>().Tracks.ToList();
            else
                tracks =
                    item.As<Artist>().Tracks.Union(item.As<Artist>().TracksThatAppearsIn)
                        .ToList();
            return tracks;
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var item = ((FrameworkElement) sender).DataContext;
            using (var lifetimeScope = App.Current.Kernel.BeginScope())
            {
                var playerService = lifetimeScope.Resolve<IPlayerService>();
                var tracks = GetTracks(item)
                        .Where(p => p.Status == TrackStatus.None || p.Status == TrackStatus.Downloading)
                        .ToList();
                await playerService.NewQueueAsync(tracks);
            }
        }

        private async void AddQueue_Click(object sender, RoutedEventArgs e)
        {
            var item = ((FrameworkElement) sender).DataContext;
            using (var scope = App.Current.Kernel.BeginScope())
            {
                var backgroundAudioService = scope.Resolve<IPlayerService>();
                try
                {
                    var tracks = GetTracks(item)
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
            var item = ((FrameworkElement) sender).DataContext;
            using (var scope = App.Current.Kernel.BeginScope())
            {
                var backgroundAudioService = scope.Resolve<IPlayerService>();
                try
                {
                    var tracks = GetTracks(item)
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
            var item = ((FrameworkElement) sender).DataContext;
            using (var scope = App.Current.Kernel.BeginScope())
            {
                var downloadService = scope.Resolve<IDownloadService>();
                var tracks = GetTracks(item).Where(p => p.IsDownloadable);
                foreach (var track in tracks)
                    downloadService.StartDownloadAsync(track);
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            var item = ((FrameworkElement) sender).DataContext;
            using (var scope = App.Current.Kernel.BeginScope())
            {
                var libraryService = scope.Resolve<ILibraryService>();
                var tracks = GetTracks(item);
                foreach (var track in tracks)
                    await libraryService.DeleteTrackAsync(track);
            }
        }

        private void ExploreArtist_Click(object sender, RoutedEventArgs e)
        {
            var item = (Album) ((FrameworkElement) sender).DataContext;
            App.Current.NavigationService.Navigate(typeof (ArtistPage), item.Artist.Name);
        }
    }
}