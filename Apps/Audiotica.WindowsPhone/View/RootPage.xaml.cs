#region

using Audiotica.Core.Utils;
using Audiotica.View.Setting;

using Windows.UI.Xaml.Navigation;

#endregion

namespace Audiotica.View
{
    public sealed partial class RootPage
    {
        public RootPage()
        {
            this.InitializeComponent();
            App.Navigator = new Navigator(this, this.LayoutRoot);

            if (App.Locator.AppVersionHelper.IsFirstRun)
            {
                App.Navigator.AddPage(new FirstRunPage());
            }
            else if (this.IsRestore())
            {
                App.Navigator.AddPage(new RestorePage());
            }

            App.Navigator.AddPage(new HomePage());
            App.Navigator.AddPage(new CollectionPage());
            App.Navigator.AddPage(new CollectionAlbumPage());
            App.Navigator.AddPage(new CollectionArtistPage());
            App.Navigator.AddPage(new CollectionPlaylistPage());
            App.Navigator.AddPage(new SpotifyAlbumPage());
            App.Navigator.AddPage(new SpotifyArtistPage());
            App.Navigator.AddPage(new ArtistPage());
            App.Navigator.AddPage(new AlbumPage());
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
            App.Navigator.AddPage(new CollectionSettingsPage());
            App.Navigator.AddPage(new CloudSubscribePage());
            App.Navigator.AddPage(new ChartsPage());
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.Locator.AppVersionHelper.IsFirstRun)
            {
                App.Navigator.GoTo<FirstRunPage, PageTransition>(null);
            }
            else if (this.IsRestore())
            {
                App.Navigator.GoTo<RestorePage, PageTransition>(null);
            }
            else
            {
                App.Navigator.GoTo<HomePage, PageTransition>(null);
            }
        }

        private bool IsRestore()
        {
            return App.Locator.AppSettingsHelper.Read<bool>("FactoryReset")
                   || App.Locator.AppSettingsHelper.Read<bool>("Restore");
        }
    }
}