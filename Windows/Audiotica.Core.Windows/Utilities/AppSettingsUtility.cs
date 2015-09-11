using Windows.Storage;
using Audiotica.Core.Utilities.Interfaces;

namespace Audiotica.Core.Windows.Utilities
{
    public class AppSettingsUtility : IAppSettingsUtility
    {
        public AppSettingsUtility(ISettingsUtility settingsUtility)
        {
            DownloadsPath = settingsUtility.Read("DownloadsPath", "virtual://Music/Audiotica/");
            TempDownloadsPath = settingsUtility.Read("TempDownloadsPath", ApplicationData.Current.TemporaryFolder.Path);
        }

        public string DownloadsPath { get; set; }

        public string TempDownloadsPath { get; }
    }
}