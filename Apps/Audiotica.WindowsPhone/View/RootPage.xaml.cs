#region

using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Utilities;
using Audiotica.Core.Utils;
using Audiotica.Core.WinRt.Utilities;
using Audiotica.View.Setting;

#endregion

namespace Audiotica.View
{
    public sealed partial class RootPage
    {
        public RootPage()
        {
            InitializeComponent();
            App.Navigator = new Navigator(this, LayoutRoot);

            if (App.Locator.AppVersionHelper.IsFirstRun)
                App.Navigator.AddPage(new FirstRunPage());
            else if (IsRestore())
                App.Navigator.AddPage(new RestorePage());

            App.Navigator.AddPage(new HomePage());
            App.Navigator.AddPage(new CollectionPage());
            App.Navigator.AddPage(new CollectionAlbumPage());
            App.Navigator.AddPage(new CollectionArtistPage());
            App.Navigator.AddPage(new CollectionPlaylistPage());
            App.Navigator.AddPage(new SpotifyAlbumPage());
            App.Navigator.AddPage(new SpotifyArtistPage());
            App.Navigator.AddPage(new SearchPage());
            App.Navigator.AddPage(new ManualMatchPage());
            App.Navigator.AddPage(new SettingsPage());
            App.Navigator.AddPage(new ApplicationPage());
            App.Navigator.AddPage(new PlayerPage());
            App.Navigator.AddPage(new DeveloperPage());
            App.Navigator.AddPage(new LastFmPage());
            App.Navigator.AddPage(new AboutPage());
            App.Navigator.AddPage(new CloudPage());
            App.Navigator.AddPage(new CollectionStatisticsPage());
            App.Navigator.AddPage(new Setting.CollectionSettingsPage());
        }

        private bool IsRestore()
        {
            return App.Locator.AppSettingsHelper.Read<bool>("FactoryReset") 
                || StorageHelper.FileExistsAsync("_current_restore.autcp").Result;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.Locator.AppVersionHelper.IsFirstRun)
                App.Navigator.GoTo<FirstRunPage, PageTransition>(null);
            else if (IsRestore())
                App.Navigator.GoTo<RestorePage, PageTransition>(null);
            else
                App.Navigator.GoTo<HomePage, PageTransition>(null);
        }
    }
}