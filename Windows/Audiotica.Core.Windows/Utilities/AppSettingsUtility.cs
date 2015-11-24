using Windows.Storage;
using Windows.UI.Xaml;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Core.Windows.Helpers;

namespace Audiotica.Core.Windows.Utilities
{
    public class AppSettingsUtility : ObservableObject, IAppSettingsUtility
    {
        private readonly ISettingsUtility _settingsUtility;
        private int _theme;
        private bool _ads;

        public AppSettingsUtility(ISettingsUtility settingsUtility)
        {
            _settingsUtility = settingsUtility;
            DownloadsPath = settingsUtility.Read("DownloadsPath", "virtual://Music/Audiotica/");
            TempDownloadsPath = settingsUtility.Read("TempDownloadsPath", ApplicationData.Current.TemporaryFolder.Path);
            _theme = _settingsUtility.Read(ApplicationSettingsConstants.Theme, (int)ElementTheme.Default);
            _ads = _settingsUtility.Read(ApplicationSettingsConstants.Ads, true);
        }

        public string DownloadsPath { get; set; }

        public string TempDownloadsPath { get; }

        public int Theme
        {
            get { return _theme; }
            set
            {
                Set(ref _theme, value);
                _settingsUtility.Write(ApplicationSettingsConstants.Theme, value);
            }
        }

        public bool Ads
        {
            get { return _ads; }
            set
            {
                Set(ref _ads, value);
                _settingsUtility.Write(ApplicationSettingsConstants.Ads, value);
            }
        }
    }
}