#region

using System;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection.Model;
using Audiotica.ViewModel;
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
            Bar = BottomAppBar;
            BottomAppBar = null;
            _bioPivotItem = BioPivot;
            _similarPivotItem = SimilarPivot;
        }

        public override void NavigatedTo(object e)
        {
            base.NavigatedTo(e);
            var id = e as int?;
            if (id == null) return;

            Messenger.Default.Send((int)id, "artist-coll-detail-id");
            Messenger.Default.Register<bool>(this, "artist-coll-bio", BioUpdate);
            Messenger.Default.Register<bool>(this, "artist-coll-sim", SimUpdate);
            Messenger.Default.Register<bool>(this, "artist-coll-pin", ToggleAppBarButton);

            ToggleAppBarButton(SecondaryTile.Exists("artist." + Vm.Artist.Id));
        }

        private CollectionArtistViewModel Vm { get { return DataContext as CollectionArtistViewModel; } }

        private void ToggleAppBarButton(bool isPinned)
        {
            if (!isPinned)
            {
                PinUnpinAppBarButton.Label = "Pin";
                PinUnpinAppBarButton.Icon = new SymbolIcon(Symbol.Pin);
            }
            else
            {
                PinUnpinAppBarButton.Label = "Unpin";
                PinUnpinAppBarButton.Icon = new SymbolIcon(Symbol.UnPin);
            }
        }


        public override void NavigatedFrom(NavigationMode mode)
        {
            base.NavigatedFrom(mode);
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

        private async void PinUnpinAppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            ToggleAppBarButton(await CollectionHelper.PinToggleAsync(Vm.Artist));
        }
    }
}