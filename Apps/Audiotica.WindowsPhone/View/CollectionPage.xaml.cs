#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using MyToolkit.Paging.Animations;
using MyToolkit.Utilities;

#endregion

namespace Audiotica.View
{
    public sealed partial class CollectionPage : IFileSavePickerContinuable, IFileOpenPickerContinuable
    {
        public CollectionPage()
        {
            InitializeComponent();
            Bar = BottomAppBar;
            BottomAppBar = null;
            Loaded += (sender, args) =>  LoadWallpaperArt();
        }

        public override void NavigatedTo(object parameter)
        {
            base.NavigatedTo(parameter);
            if (parameter == null) return;

            LoadWallpaperArt();

            var pivotIndex = (int)parameter;
            CollectionPivot.SelectedIndex = pivotIndex;
        }

        private async void LoadWallpaperArt()
        {
            var vm = App.Locator.Collection;

            if (vm.RandomizeAlbumList.Count != 0 || !AppSettingsHelper.Read("WallpaperArt", true, SettingsStrategy.Roaming)) return;

            var albums =
                App.Locator.CollectionService.Albums.ToList()
                    .Where(p => p.Artwork != CollectionConstant.MissingArtworkImage)
                    .ToList();

            var albumCount = albums.Count;

            if (albumCount < 10) return;

            var h = Window.Current.Bounds.Height;
            var rows = (int)Math.Ceiling(h / (ActualWidth / 5));

            var numImages = rows*5;
            var imagesNeeded = numImages - albumCount;

            var shuffle = await Task.FromResult(albums
                .Shuffle()
                .Take(numImages > albumCount ? albumCount : numImages)
                .ToList());

            if (imagesNeeded > 0)
            {
                var repeatList = new List<Album>();

                while (imagesNeeded > 0)
                {
                    var takeAmmount = imagesNeeded > albumCount ? albumCount : imagesNeeded;

                    await Task.Run(() => repeatList.AddRange(shuffle.Shuffle().Take(takeAmmount)));

                    imagesNeeded -= shuffle.Count;
                }

                shuffle.AddRange(repeatList);
            }

            vm.RandomizeAlbumList.AddRange(shuffle);
        }

        private void CollectionPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (Bar as CommandBar).ClosedDisplayMode =
                CollectionPivot.SelectedIndex == 3 ? AppBarClosedDisplayMode.Compact : AppBarClosedDisplayMode.Minimal;
        }

        public async void ContinueFileSavePicker(FileSavePickerContinuationEventArgs args)
        {
            var file = args.File;

            if (file == null)
            {
                CurtainPrompt.ShowError("Backup cancelled.");
                return;
            }

            UiBlockerUtility.Block("Backing up (this may take a bit)...");

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
            UiBlockerUtility.Unblock();

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

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<NewPlaylistPage, ZoomOutTransition>(null);
        }

        private async void ImportButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            UiBlockerUtility.Block("Scanning...");
            var localMusic = await LocalMusicHelper.GetFilesInMusic();
            var failedCount = 0;

            App.Locator.CollectionService.Songs.SuppressEvents = true;
            App.Locator.CollectionService.Artists.SuppressEvents = true;
            App.Locator.CollectionService.Albums.SuppressEvents = true;

            App.Locator.SqlService.DbConnection.BeginTransaction();
            for (var i = 0; i < localMusic.Count; i++)
            {
                StatusBarHelper.ShowStatus(string.Format("Importing {0} of {1} items", i + 1, localMusic.Count), (double)i / localMusic.Count);
                try
                {
                    await LocalMusicHelper.SaveTrackAsync(localMusic[i]);
                }
                catch
                {
                    failedCount++;
                }
            }
            App.Locator.SqlService.DbConnection.Commit();

            App.Locator.CollectionService.Songs.Reset();
            App.Locator.CollectionService.Artists.Reset();
            App.Locator.CollectionService.Albums.Reset();

            UiBlockerUtility.Unblock();

            if (failedCount > 0)
                CurtainPrompt.ShowError("Couldn't import {0} song(s).", failedCount);
            CollectionHelper.DownloadArtistsArtworkAsync();
        }

        private void BackupButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Audiotica Backup", new List<string>() { ".autcp" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = string.Format("{0}-WP81", (int)DateTime.Now.ToUnixTimeStamp());

            savePicker.PickSaveFileAndContinue();
        }

        private async void RestoreButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (await MessageBox.ShowAsync("This will delete all your pre-existing data.", "Continue with Restore?",
               MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            var fileOpenPicker = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
            fileOpenPicker.FileTypeFilter.Add(".autcp");
            fileOpenPicker.PickSingleFileAndContinue();
        }
    }
}