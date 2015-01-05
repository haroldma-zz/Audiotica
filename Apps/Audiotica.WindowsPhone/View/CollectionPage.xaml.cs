#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;
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
    public sealed partial class CollectionPage : IFileSavePickerContinuable, IFileOpenPickerContinuable
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

        private async void ImportAppBarButton_Click_2(object sender, RoutedEventArgs e)
        {
            StatusBarHelper.ShowStatus("Scanning...");
            var localMusic = await LocalMusicHelper.GetFilesInMusic();

            for (var i = 0; i < localMusic.Count; i++)
            {
                StatusBarHelper.ShowStatus(string.Format("{0} of {1} items added", i + 1, localMusic.Count), (double)i / localMusic.Count);
                await LocalMusicHelper.SaveTrackAsync(localMusic[i]);
            }

            StatusBarHelper.HideStatus();
            await CollectionHelper.DownloadArtistsArtworkAsync();
        }

        private void AppBarButton_Click_3(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Audiotica Backup", new List<string>() { ".autcp" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = string.Format("{0}-WP81", DateTime.Now.ToString("yy-MM-dd"));

            savePicker.PickSaveFileAndContinue();
        }

        private async void AppBarButton_Click_2(object sender, RoutedEventArgs e)
        {
            if (await MessageBox.ShowAsync("This will delete all your pre-existing data.", "Continue with Restore?", 
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            var fileOpenPicker = new FileOpenPicker {SuggestedStartLocation = PickerLocationId.DocumentsLibrary};
            fileOpenPicker.FileTypeFilter.Add(".autcp");
            fileOpenPicker.PickSingleFileAndContinue();
        }

        public async void ContinueFileSavePicker(FileSavePickerContinuationEventArgs args)
        {
            var file = args.File;

            if (file == null)
            {
                CurtainPrompt.ShowError("Backup cancelled.");
                return;
            }

            StatusBarHelper.ShowStatus("Backing up (this may take a bit)...");

            await StorageHelper.DeleteFileAsync("collection.bksqldb");
            await StorageHelper.DeleteFileAsync("player.bksqldb");

            var sqlFile = await StorageHelper.GetFileAsync("collection.sqldb");
            var playerSqlFile = await StorageHelper.GetFileAsync("player.sqldb");
            await sqlFile.CopyAsync(ApplicationData.Current.LocalFolder, "collection.bksqldb");
            await playerSqlFile.CopyAsync(ApplicationData.Current.LocalFolder, "player.bksqldb");

            var data = await AutcpFormatHelper.CreateBackup(ApplicationData.Current.LocalFolder);
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                await stream.WriteAsync(data, 0, data.Length);
            }
            StatusBarHelper.HideStatus();

            CurtainPrompt.Show("Backup completed.");
        }

        public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            var file = args.Files.FirstOrDefault();

            if (file == null)
            {
                CurtainPrompt.ShowError("No backup file picked.");
                return;
            }

           
            StatusBarHelper.ShowStatus("Preparing...");
            using (var stream = await file.OpenStreamForReadAsync())
            {
                if (AutcpFormatHelper.ValidateHeader(stream))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    var restoreFile = await StorageHelper.CreateFileAsync("_current_restore.autcp");

                    using (var restoreStream = await restoreFile.OpenStreamForWriteAsync())
                    {
                        await stream.CopyToAsync(restoreStream);
                    }

                    StatusBarHelper.HideStatus();
                    await
                        MessageBox.ShowAsync(
                            "To finish applying the restore the app will close. Next time you start the app, it will finish restoring.",
                            "Application Restart Required");

                    App.Locator.AudioPlayerHelper.FullShutdown();
                    Application.Current.Exit();
                }
                else
                {
                    CurtainPrompt.ShowError("Not a valid backup file.");
                }
            }
        }
    }
}