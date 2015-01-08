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
using MyToolkit.Utilities;

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
            var rows = (int)Math.Ceiling(h / (WallpaperGridView.ActualWidth / 5));

            var numImages = rows*5;
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
    }
}