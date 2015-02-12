#region

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Data.Spotify.Models;
using GalaSoft.MvvmLight.Messaging;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.View
{
    public sealed partial class SpotifyArtistPage
    {
        public SpotifyArtistPage()
        {
            InitializeComponent();
        }

        public override void NavigatedTo(Windows.UI.Xaml.Navigation.NavigationMode mode, object parameter)
        {
            base.NavigatedTo(mode, parameter);
            var id = parameter as string;

            if (id == null) return;

            var msg = new GenericMessage<string>(id);
            Messenger.Default.Send(msg, "spotify-artist-detail-id");
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as SimpleAlbum;
            if (album != null) App.Navigator.GoTo<SpotifyAlbumPage, ZoomInTransition>(album.Id);
        }
    }
}