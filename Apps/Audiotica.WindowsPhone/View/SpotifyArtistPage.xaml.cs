#region

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Data.Spotify.Models;
using Audiotica.ViewModel;
using GalaSoft.MvvmLight.Messaging;

#endregion

namespace Audiotica.View
{
    public sealed partial class SpotifyArtistPage
    {
        public SpotifyArtistPage()
        {
            InitializeComponent();
        }

        public override void NavigatedTo(NavigationMode mode, object parameter)
        {
            base.NavigatedTo(mode, parameter);
            var id = parameter as string;

            if (id == null) return;

            var msg = new GenericMessage<string>(id);
            Messenger.Default.Send(msg, "spotify-artist-detail-id");
        }

        public override void NavigatedFrom(NavigationMode mode)
        {
            base.NavigatedFrom(mode);
            if (mode != NavigationMode.Back) return;

            var vm = DataContext as SpotifyArtistViewModel;
            vm.Artist = null;
            vm.TopAlbums = null;
            vm.TopTracks = null;
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as SimpleAlbum;
            if (album != null) App.Navigator.GoTo<SpotifyAlbumPage, ZoomInTransition>(album.Id);
        }
    }
}