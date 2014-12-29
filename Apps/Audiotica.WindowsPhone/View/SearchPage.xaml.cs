using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Data.Spotify.Models;
using IF.Lastfm.Core.Objects;

namespace Audiotica.View
{
    public sealed partial class SearchPage
    {
        public SearchPage()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
            {
                if (!_isBack)
                    SearchTextBox.Focus(FocusState.Keyboard);
            };
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as SimpleAlbum;
            Frame.Navigate(typeof(SpotifyAlbumPage), album.Id);
        }

        private void ListView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            var artist = e.ClickedItem as SimpleArtist;
            Frame.Navigate(typeof(SpotifyArtistPage), artist.Id);
        }

        private bool _isBack;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _isBack = e.NavigationMode == NavigationMode.Back;
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchTextBox.SelectAll();
        }
    }
}