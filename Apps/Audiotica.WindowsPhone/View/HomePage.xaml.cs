#region

using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Microsoft.Xbox.Music.Platform.Contract.DataModel;

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
                              != "1409-beta1-patch3";
            else
            {
                ApplicationData.Current.LocalSettings.Values.Add("CurrentBuild", "1409-beta1-patch3");
                new MessageDialog("This beta is meant for UI feedback and international testing of the charts api.", 
                    "v1409 (BETA1) - Patch #3").ShowAsync();
            }

            if (!justUpdated || firstRun) return;
            new MessageDialog("changelog here", "Patch #").ShowAsync();
            ApplicationData.Current.LocalSettings.Values["CurrentBuild"] = "1409-beta1-patch3";
        }

        //TODO [Harry,20140908] move this to view model with RelayCommand
        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as XboxAlbum;
            if (album != null) Frame.Navigate(typeof (AlbumPage), album.Id);
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var artist = ((Grid) sender).DataContext as XboxArtist;
            if (artist != null) Frame.Navigate(typeof (ArtistPage), artist.Id);
        }
    }
}