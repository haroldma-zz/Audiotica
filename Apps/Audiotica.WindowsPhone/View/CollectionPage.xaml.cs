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
        public CollectionPage()
        {
            InitializeComponent();

            Loaded += (sender, args) => LoadWallpaperArt();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.Back) return;

            var pivotIndex = int.Parse(e.Parameter.ToString());
            CollectionPivot.SelectedIndex = pivotIndex;
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

            if (albumCount < 10) return;

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
                    CollectionHelper.AddToQueueAsync(song);
                }
            };
            flyout.ItemsSource = list;

            FlyoutBase.ShowAttachedFlyout(menu);
        }

        private void DownloadAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var song = (sender as AppBarButton).DataContext as Song;
            App.Locator.Download.StartDownloadAsync(song);
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
            var artist = (sender as AppBarButton).DataContext as Artist;
            var queueSong = artist.Songs.ToList().Shuffle().ToList();
            await CollectionHelper.PlaySongsAsync(queueSong);
        }

        private async void AlbumPlayAppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            var album = (sender as AppBarButton).DataContext as Album;
            var queueSong = album.Songs.ToList().Shuffle().ToList();
            await CollectionHelper.PlaySongsAsync(queueSong);
        }

        private async void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            var playlist = (sender as AppBarButton).DataContext as Playlist;
            var queueSong = playlist.Songs.Select(p => p.Song).ToList();
            await CollectionHelper.PlaySongsAsync(queueSong);
        }

        private async void AppBarButton_Click_2(object sender, RoutedEventArgs e)
        {
            StatusBarHelper.ShowStatus("Scanning...");
            var localMusic = await LocalMusicHelper.GetFilesInMusic();

            for (var i = 0; i < localMusic.Count; i++)
            {
                StatusBarHelper.ShowStatus(string.Format("{0} of {1} items added", i + 1, localMusic.Count));
                await LocalMusicHelper.SaveTrackAsync(localMusic[i]);
            }

            StatusBarHelper.HideStatus();
        }
    }
}