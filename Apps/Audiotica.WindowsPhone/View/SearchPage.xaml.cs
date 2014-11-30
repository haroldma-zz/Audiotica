using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using IF.Lastfm.Core.Objects;

namespace Audiotica.View
{
    public sealed partial class SearchPage
    {
        public SearchPage()
        {
            InitializeComponent();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as LastAlbum;
            Frame.Navigate(typeof(AlbumPage), album);
        }

        private void ListView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            var artist = e.ClickedItem as LastArtist;
            Frame.Navigate(typeof(ArtistPage), artist.Name);
        }

        private void PageBase_Loaded(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Focus(FocusState.Keyboard);
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchTextBox.SelectAll();
        }
    }
}