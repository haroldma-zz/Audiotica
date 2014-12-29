#region

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Data.Model.Spotify;
using Audiotica.Data.Model.Spotify.Models;
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

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var id = e.Parameter as string;

            if (id == null) return;

            var msg = new GenericMessage<string>(id);
            Messenger.Default.Send(msg, "spotify-artist-detail-id");
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as SimpleAlbum;
            if (album != null) Frame.Navigate(typeof (SpotifyAlbumPage), album.Id);
        }
    }
}