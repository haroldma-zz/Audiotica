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

        private async void PageBase_Loaded(object sender, RoutedEventArgs e)
        {
            StatusBarHelper.ShowStatus("Restoring (this may take a bit)...");

            var file = await StorageHelper.GetFileAsync("_current_restore.autcp");

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
            Frame.Navigate(typeof (HomePage));
        }
    }
}