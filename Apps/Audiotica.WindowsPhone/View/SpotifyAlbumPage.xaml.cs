#region

using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Common;
using Audiotica.ViewModel;
using GalaSoft.MvvmLight.Messaging;

#endregion

namespace Audiotica.View
{
    public sealed partial class SpotifyAlbumPage
    {
        public SpotifyAlbumPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var album = e.Parameter as string;

            if (album == null) return;

            var msg = new GenericMessage<string>(album);
            Messenger.Default.Send(msg, "spotify-album-detail");
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = (sender as HyperlinkButton).DataContext as AlbumViewModel;

            Frame.Navigate(typeof (ArtistPage), vm.Album.ArtistName);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = (sender as Button).DataContext as SpotifyAlbumViewModel;
            CollectionHelper.SaveAlbumAsync(vm.Album);
        }
    }
}