#region

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Audiotica.Data.Collection.Model;
using GalaSoft.MvvmLight.Messaging;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.View
{
    public sealed partial class CollectionArtistPage
    {
        private readonly PivotItem _bioPivotItem;
        private readonly PivotItem _similarPivotItem;

        public CollectionArtistPage()
        {
            InitializeComponent();
            _bioPivotItem = BioPivot;
            _similarPivotItem = SimilarPivot;
        }

        public override void NavigatedTo(object e)
        {
            base.NavigatedTo(e);
            var id = e as long?;
            if (id == null) return;

            Messenger.Default.Send((long) id, "artist-coll-detail-id");
            Messenger.Default.Register<bool>(this, "artist-coll-bio", BioUpdate);
            Messenger.Default.Register<bool>(this, "artist-coll-sim", SimUpdate);
        }

        public override void NavigatedFrom()
        {
            base.NavigatedFrom();
            Messenger.Default.Unregister<bool>(this, "artist-coll-bio", BioUpdate);
            Messenger.Default.Unregister<bool>(this, "artist-coll-sim", SimUpdate);
        }

        private void SimUpdate(bool isVisible)
        {
            if (isVisible)
                if (!ArtistPivot.Items.Contains(_similarPivotItem))
                    ArtistPivot.Items.Add(_similarPivotItem);
                else
                    ArtistPivot.Items.Remove(SimilarPivot);
        }

        private void BioUpdate(bool isVisible)
        {
            if (isVisible)
                if (!ArtistPivot.Items.Contains(_bioPivotItem))
                    ArtistPivot.Items.Add(_bioPivotItem);
                else
                {
                    ArtistPivot.Items.Remove(BioPivot);
                }
        }

        private void AlbumListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as Album;
            App.Navigator.GoTo<CollectionAlbumPage, ZoomInTransition>(album.Id);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var artist = e.ClickedItem as LastArtist;
            App.Navigator.GoTo<SpotifyArtistPage, ZoomInTransition>("name." + artist.Name);
        }
    }
}