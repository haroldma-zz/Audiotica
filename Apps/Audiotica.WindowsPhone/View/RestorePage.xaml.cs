#region

using System;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;

#endregion

namespace Audiotica.View
{
    public sealed partial class RestorePage
    {
        public RestorePage()
        {
            InitializeComponent();
        }

        public override async void NavigatedTo(object parameter)
        {
            base.NavigatedTo(parameter);
            StatusBarHelper.ShowStatus("Restoring (this may take a bit)...");

            var file = await StorageHelper.GetFileAsync("_current_restore.autcp");

            //delete artowkr and mp3s
            var artworkFolder = await StorageHelper.GetFolderAsync("artworks");
            var artistFolder = await StorageHelper.GetFolderAsync("artists");
            var songFolder = await StorageHelper.GetFolderAsync("songs");

            if (artworkFolder != null)
            {
                await artworkFolder.DeleteAsync();
            }

            if (artistFolder != null)
            {
                await artistFolder.DeleteAsync();
            }

            if (songFolder != null)
            {
                await songFolder.DeleteAsync();
            }

            using (var stream = await file.OpenStreamForWriteAsync())
            {
                await AutcpFormatHelper.UnpackBackup(ApplicationData.Current.LocalFolder, stream);

                var coll = await StorageHelper.GetFileAsync("collection.bksqldb");
                var player = await StorageHelper.GetFileAsync("player.bksqldb");
                await coll.CopyAndReplaceAsync(await StorageHelper.GetFileAsync("collection.sqldb"));
                await player.CopyAndReplaceAsync(await StorageHelper.GetFileAsync("player.sqldb"));

                //cleanup
                await StorageHelper.DeleteFileAsync("collection.bksqldb");
                await StorageHelper.DeleteFileAsync("player.bksqldb");
            }

            await file.DeleteAsync();

            StatusBarHelper.HideStatus();
            CurtainPrompt.Show("Finish restoring.");
            (Application.Current as App).BootAppServicesAsync();
            App.Navigator.GoTo<HomePage, ZoomOutTransition>(null);
        }
    }
}