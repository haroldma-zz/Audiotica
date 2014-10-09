#region

using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Utilities;
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