#region

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.View
{
    public sealed partial class AlbumPage
    {
        public AlbumPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var album = e.Parameter as LastAlbum;

            if (album == null) return;

            var msg = new GenericMessage<LastAlbum>(album);
            Messenger.Default.Send(msg, "album-detail");
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = (sender as HyperlinkButton).DataContext as AlbumViewModel;

            Frame.Navigate(typeof (ArtistPage), vm.Album.ArtistName
                );
        }
    }
}