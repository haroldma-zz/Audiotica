#region

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Core.Utils;
using Audiotica.Core.WinRt.Utilities;
using PCLStorage;
using Xamarin;

#endregion

namespace Audiotica.View
{
    public sealed partial class RestorePage
    {
        public RestorePage()
        {
            InitializeComponent();
        }

        public async override void NavigatedTo(Windows.UI.Xaml.Navigation.NavigationMode mode, object parameter)
        {
            base.NavigatedTo(mode, parameter);
            
            var reset = App.Locator.AppSettingsHelper.Read<bool>("FactoryReset");

            var startingMsg = "Restoring (this may take a bit)...";
            if (reset)
                startingMsg = "Factory resetting...";

            using (Insights.TrackTime(reset ? "Factory Reset" : "Restore Collection"))
            {
                StatusBarHelper.ShowStatus(startingMsg);

                var file = reset ? null : await StorageHelper.GetFileAsync("_current_restore.autcp");

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

                if (!reset)
                {
                    using (var stream = await file.OpenAsync(FileAccess.ReadAndWrite))
                    {
                        await AutcpFormatHelper.UnpackBackup(ApplicationData.Current.LocalFolder, stream);
                    }

                    await file.DeleteAsync();

                    App.Locator.CollectionService.LibraryLoaded += async (sender, args) =>
                    {
                        await CollectionHelper.DownloadArtistsArtworkAsync(false);
                    };

                    App.Locator.AppSettingsHelper.Write("Restore", false);
                }
                else
                {
                    var dbs = (await ApplicationData.Current.LocalFolder.GetFilesAsync())
                    .Where(p => p.FileType == ".sqldb").ToList();

                    foreach (var db in dbs)
                    {
                        await db.DeleteAsync();
                    }

                    App.Locator.AppSettingsHelper.Write("FactoryReset", false);
                }

                StatusBarHelper.HideStatus();
            }

            (Application.Current as App).BootAppServicesAsync();
            App.Navigator.GoTo<HomePage, ZoomOutTransition>(null, false);
        }
    }
}