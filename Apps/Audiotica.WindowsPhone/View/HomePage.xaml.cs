#region

using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.View
{
    public sealed partial class HomePage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var justUpdated = true;
            var firstRun = !ApplicationData.Current.LocalSettings.Values.ContainsKey("CurrentBuild");

            if (!firstRun)
                justUpdated = (string) ApplicationData.Current.LocalSettings.Values["CurrentBuild"]
                              != "1409-beta3-patch1";
            else
            {
                ApplicationData.Current.LocalSettings.Values.Add("CurrentBuild", "1409-beta3-patch1");
                new MessageDialog(
                    "Test out saving, deleting and playing songs",
                    "v1409 (BETA3)").ShowAsync();
            }

            if (!justUpdated || firstRun) return;
            new MessageDialog(
                "-switch from xbox music to last.fm\n-now playing page is everywhere!\n-subtle changes in ui",
                "Beta3 - Patch #1")
                .ShowAsync();
            ApplicationData.Current.LocalSettings.Values["CurrentBuild"] = "1409-beta3-patch1";
        }

        //TODO [Harry,20140908] move this to view model with RelayCommand
        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as LastAlbum;
            if (album != null) Frame.Navigate(typeof (AlbumPage), album);
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var artist = ((Grid) sender).DataContext as LastArtist;
            if (artist != null) Frame.Navigate(typeof (ArtistPage), artist.Name);
        }
    }
}