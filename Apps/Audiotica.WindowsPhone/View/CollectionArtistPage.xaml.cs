using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Data.Collection.Model;
using GalaSoft.MvvmLight.Messaging;

namespace Audiotica.View
{
    public sealed partial class CollectionArtistPage
    {
        public CollectionArtistPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var id = e.Parameter as long?;

            if (id == null) return;

            var msg = new GenericMessage<long>((long)id);
            Messenger.Default.Send(msg, "artist-coll-detail-id");
        }

        private void AlbumListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as Album;
            Frame.Navigate(typeof(CollectionAlbumPage), album.Id);
        }
    }
}