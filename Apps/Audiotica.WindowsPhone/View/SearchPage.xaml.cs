using Audiotica.Data.Spotify.Models;
using Audiotica.ViewModel;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Audiotica.View
{
    public sealed partial class SearchPage
    {
        public SearchPage()
        {
            InitializeComponent();
        }

        public override async void NavigatedTo(NavigationMode mode, object parameter)
        {
            base.NavigatedTo(mode, parameter);

            if (mode == NavigationMode.Back)
            {
                return;
            }

            var term = parameter as string;
            if (!string.IsNullOrEmpty(term))
            {
                var vm = (SearchViewModel)DataContext;
                SearchTextBox.Text = term;
                SearchTextBox.IsEnabled = false;
                vm.IsLoading = true;
                await vm.SearchAsync(term);
                SearchTextBox.IsEnabled = true;
                vm.IsLoading = false;
            }
            else
            {
                SearchTextBox.Focus(FocusState.Keyboard);
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as SimpleAlbum;
            App.Navigator.GoTo<SpotifyAlbumPage, ZoomInTransition>(album.Id);
        }

        private void ListView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            var artist = e.ClickedItem as SimpleArtist;
            App.Navigator.GoTo<SpotifyArtistPage, ZoomInTransition>(artist.Id);
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchTextBox.SelectAll();
        }
    }
}