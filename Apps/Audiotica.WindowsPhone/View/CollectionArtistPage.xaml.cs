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

            ToggleAppBarButton(!SecondaryTile.Exists(ArtistTileId));
        }

        private CollectionArtistViewModel Vm { get { return DataContext as CollectionArtistViewModel; } }

        private string ArtistTileId
        {
            get { return "artist." + Vm.Artist.Id; }
        }

        private void ToggleAppBarButton(bool showPinButton)
        {
            if (showPinButton)
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
            bool showPinButton;
            if (!SecondaryTile.Exists(ArtistTileId))
            {
                var displayName = Vm.Artist.Name;
                var tileActivationArguments = "artists/" + Vm.Artist.Id;
                var image =
                    new Uri(CollectionConstant.LocalStorageAppPath +
                            string.Format(CollectionConstant.ArtistsArtworkPath, Vm.Artist.Id));

                var secondaryTile = new SecondaryTile(ArtistTileId,
                    displayName,
                    tileActivationArguments,
                    image,
                    TileSize.Square150x150)
                {
                    ShortName = displayName
                };
                secondaryTile.VisualElements.ShowNameOnSquare150x150Logo = true;

                showPinButton = !await secondaryTile.RequestCreateAsync();
            }
            else
            {
                var secondaryTile = new SecondaryTile(ArtistTileId);
                showPinButton = await secondaryTile.RequestDeleteAsync();
            }
            ToggleAppBarButton(showPinButton);
        }
    }
}