#region

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Utilities;

#endregion

namespace Audiotica.View
{
    public sealed partial class RootPage
    {
        private bool IsRestore()
        {
            return StorageHelper.FileExistsAsync("_current_restore.autcp").Result;
        }

        public RootPage()
        {
            InitializeComponent();
            App.Navigator = new Navigator(this, LayoutRoot);
            
            if (AppVersionHelper.IsFirstRun)
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
            App.Navigator.AddPage(new SettingsPage());
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (AppVersionHelper.IsFirstRun)
                App.Navigator.GoTo<FirstRunPage, PageTransition>(null);
            else if (IsRestore())
                App.Navigator.GoTo<RestorePage, PageTransition>(null);
            else
                App.Navigator.GoTo<HomePage, PageTransition>(null);
        }
    }
}