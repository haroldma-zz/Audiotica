using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Data.Collection.Model;
using GalaSoft.MvvmLight.Messaging;
using IF.Lastfm.Core.Objects;

namespace Audiotica.View
{
    public sealed partial class CollectionArtistPage
    {
        public CollectionArtistPage()
        {
            InitializeComponent();
            _bioPivotItem = BioPivot;
            _similarPivotItem = SimilarPivot;

            Messenger.Default.Register<bool>(this, "artist-coll-bio", BioUpdate);
            Messenger.Default.Register<bool>(this, "artist-coll-sim", SimUpdate);
        }

        private PivotItem _bioPivotItem;
        private PivotItem _similarPivotItem;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var id = e.Parameter as long?;
            if (id == null) return;

            Messenger.Default.Send((long)id, "artist-coll-detail-id");
        }

        private void SimUpdate(bool isVisible)
        {
            if (isVisible)
                ArtistPivot.Items.Add(_similarPivotItem);
            else
                ArtistPivot.Items.Remove(SimilarPivot);
        }

        private void BioUpdate(bool isVisible)
        {
            if (isVisible)
            {
                ArtistPivot.Items.Add(_bioPivotItem);
            }
            else
            {
                ArtistPivot.Items.Remove(BioPivot);                
            }
        }

        private void AlbumListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as Album;
            Frame.Navigate(typeof(CollectionAlbumPage), album.Id);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var artist = e.ClickedItem as LastArtist;
            Frame.Navigate(typeof(ArtistPage), artist.Name);
        }
    }
}