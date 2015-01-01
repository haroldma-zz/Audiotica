#region

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection.Model;
using Audiotica.ViewModel;

#endregion

namespace Audiotica.View
{
    public sealed partial class CollectionPage
    {
        private bool _currentlyPreparing;

        public CollectionPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                var pivotIndex = int.Parse(e.Parameter.ToString());
                CollectionPivot.SelectedIndex = pivotIndex;
            }

            LoadWallpaperArt();
        }

        private void LoadWallpaperArt()
        {
            var vm = (CollectionViewModel) DataContext;

            if (vm.RandomizeAlbumList.Count != 0 || !AppSettingsHelper.Read("WallpaperArt", true)) return;

            var albums =
                App.Locator.CollectionService.Albums.ToList()
                    .Where(p => p.Artwork != CollectionConstant.MissingArtworkImage)
                    .ToList();

            var albumCount = albums.Count;

            if (albumCount <= 10) return;

            var h = Window.Current.Bounds.Height;
            var rows = (int) Math.Ceiling(h/vm.ArtworkSize);

            var numImages = rows*vm.RowCount;
            var imagesNeeded = numImages - albumCount;

            var shuffle = albums
                .Shuffle()
                .Take(numImages > albumCount ? albumCount : numImages)
                .ToList();

            if (imagesNeeded > 0)
            {
                var repeatList = new List<Album>();

                while (imagesNeeded > 0)
                {
                    var takeAmmount = imagesNeeded > albumCount ? albumCount : imagesNeeded;

                    repeatList.AddRange(shuffle.Shuffle().Take(takeAmmount));

                    imagesNeeded -= shuffle.Count;
                }

                shuffle.AddRange(repeatList);
            }

            vm.RandomizeAlbumList.AddRange(shuffle);
        }

        private void AlbumListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as Album;
            Frame.Navigate(typeof (CollectionAlbumPage), album.Id);
        }

        private void ArtistListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var artist = e.ClickedItem as Artist;
            Frame.Navigate(typeof (CollectionArtistPage), artist.Id);
        }

        private void PlaylistListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var playlist = e.ClickedItem as Playlist;
            Frame.Navigate(typeof (CollectionPlaylistPage), playlist.Id);
        }

        private async void DeleteSongMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var song = (Song) ((FrameworkElement) sender).DataContext;

            try
            {
                //delete from the queue
                await App.Locator.CollectionService.DeleteFromQueueAsync(song);

                //stop playback
                if (song.Id == AppSettingsHelper.Read<long>(PlayerConstants.CurrentTrack))
                    await App.Locator.AudioPlayerHelper.ShutdownPlayerAsync();

                await App.Locator.CollectionService.DeleteSongAsync(song);
                CurtainPrompt.Show("EntryDeletingSuccess".FromLanguageResource(), song.Name);
            }
            catch
            {
                CurtainPrompt.ShowError("EntryDeletingError".FromLanguageResource(), song.Name);
            }
        }

        private void CollectionPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (BottomAppBar as CommandBar).ClosedDisplayMode =
                CollectionPivot.SelectedIndex == 3 ? AppBarClosedDisplayMode.Compact : AppBarClosedDisplayMode.Minimal;
        }

        private void PickerFlyout_Confirmed(PickerFlyout sender, PickerConfirmedEventArgs args)
        {
            var listView = (ListView) sender.Content;
            var selection = listView.SelectedItems.Select(o => (Song) o).ToList();

            if (selection.Count > 0)
            {
                Frame.Navigate(typeof (NewPlaylistPage), selection);
            }
            else
            {
                CurtainPrompt.ShowError("SongsNoneSelected".FromLanguageResource());
            }
        }

        private void PickerFlyout_Closed(object sender, object e)
        {
            var listView = (ListView) ((PickerFlyout) sender).Content;
            listView.SelectedIndex = -1;
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var menu = (MenuFlyoutItem) sender;
            var flyout = (ListPickerFlyout) FlyoutBase.GetAttachedFlyout(menu);
            var song = (Song) menu.DataContext;

            var list = new List<CollectionViewModel.AddableCollectionItem>
            {
                new CollectionViewModel.AddableCollectionItem
                {
                    Name = "NowPlayingName".FromLanguageResource()
                }
            };

            list.AddRange(App.Locator.CollectionService
                .Playlists.Where(p => p.Songs.Count(pp => pp.SongId == song.Id) == 0)
                .Select(p => new CollectionViewModel.AddableCollectionItem
                {
                    Playlist = p,
                    Name = p.Name
                }));
            flyout.ItemsPicked += (pickerFlyout, args) =>
            {
                var item = args.AddedItems[0] as CollectionViewModel.AddableCollectionItem;

                if (item.Playlist != null)
                {
                    App.Locator.CollectionService.AddToPlaylistAsync(item.Playlist, song);
                }
                else
                {
                    App.Locator.CollectionService.AddToQueueAsync(song);
                }
            };
            flyout.ItemsSource = list;

            FlyoutBase.ShowAttachedFlyout(menu);
        }

        private void DownloadAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var song = (sender as AppBarButton).DataContext as Song;
            App.Locator.Download.StartDownload(song);
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var song = (sender as AppBarButton).DataContext as Song;
            App.Locator.Download.Cancel(song.Download);
        }

        private async void DeleteMenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)
        {
            var playlist = (Playlist) ((FrameworkElement) sender).DataContext;

            try
            {
                await App.Locator.CollectionService.DeletePlaylistAsync(playlist);
                CurtainPrompt.Show("EntryDeletingSuccess".FromLanguageResource(), playlist.Name);
            }
            catch
            {
                CurtainPrompt.ShowError("EntryDeletingError".FromLanguageResource(), playlist.Name);
            }
        }

        private async void ArtistPlayAppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (_currentlyPreparing) return;
            _currentlyPreparing = true;

            var artist = (sender as AppBarButton).DataContext as Artist;
            if (artist.Songs.Count == 0) return;

            var index = artist.Songs.Count == 1 ? 0 : new Random().Next(0, artist.Songs.Count - 1);

            await App.Locator.CollectionService.ClearQueueAsync().ConfigureAwait(false);

            foreach (var song in artist.Songs.ToList().Shuffle())
            {
                await App.Locator.CollectionService.AddToQueueAsync(song);
            }

            App.Locator.AudioPlayerHelper.PlaySong(App.Locator.CollectionService.PlaybackQueue[index]);
            _currentlyPreparing = false;
        }

        private async void AlbumPlayAppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (_currentlyPreparing) return;
            _currentlyPreparing = true;

            var album = (sender as AppBarButton).DataContext as Album;
            if (album.Songs.Count == 0) return;

            var index = album.Songs.Count == 1 ? 0 : new Random().Next(0, album.Songs.Count - 1);

            await App.Locator.CollectionService.ClearQueueAsync().ConfigureAwait(false);

            foreach (var song in album.Songs.ToList().Shuffle())
            {
                await App.Locator.CollectionService.AddToQueueAsync(song);
            }

            App.Locator.AudioPlayerHelper.PlaySong(App.Locator.CollectionService.PlaybackQueue[index]);
            _currentlyPreparing = false;
        }

        private async void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (_currentlyPreparing) return;
            _currentlyPreparing = true;

            var playlist = (sender as AppBarButton).DataContext as Playlist;
            if (playlist.Songs.Count == 0) return;

            var index = playlist.Songs.Count == 1 ? 0 : new Random().Next(0, playlist.Songs.Count - 1);

            await App.Locator.CollectionService.ClearQueueAsync().ConfigureAwait(false);

            foreach (var song in playlist.Songs.ToList())
            {
                await App.Locator.CollectionService.AddToQueueAsync(song.Song);
            }

            App.Locator.AudioPlayerHelper.PlaySong(App.Locator.CollectionService.PlaybackQueue[index]);
            _currentlyPreparing = false;
        }
    }
}