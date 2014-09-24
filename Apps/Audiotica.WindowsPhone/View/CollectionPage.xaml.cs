using Windows.UI.Xaml.Controls;
using Audiotica.Data.Collection.Model;

namespace Audiotica.View
{
    public sealed partial class CollectionPage
    {
        public CollectionPage()
        {
            InitializeComponent();
        }

        private void AlbumListView_ItemClick(object sender,ItemClickEventArgs e)
        {
            var album = e.ClickedItem as Album;
            Frame.Navigate(typeof (CollectionAlbumPage), album.Id);
        }
    }
}