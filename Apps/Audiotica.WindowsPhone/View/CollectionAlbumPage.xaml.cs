#region

using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Audiotica.ViewModel;
using GalaSoft.MvvmLight.Messaging;

#endregion

namespace Audiotica.View
{
    public sealed partial class CollectionAlbumPage
    {
        public CollectionAlbumPage()
        {
            InitializeComponent();
            Bar = BottomAppBar;
            BottomAppBar = null;
        }

        public override void NavigatedTo(object e)
        {
            base.NavigatedTo(e);
            var id = e as int?;

            if (id == null) return;

            var msg = new GenericMessage<int>((int)id);
            Messenger.Default.Send(msg, "album-coll-detail-id");

            ToggleAppBarButton(SecondaryTile.Exists("album." + Vm.Album.Id));
        }

        private CollectionAlbumViewModel Vm { get { return DataContext as CollectionAlbumViewModel; } }

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

        private async void PinUnpinAppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            ToggleAppBarButton(await CollectionHelper.PinToggleAsync(Vm.Album));
        }
    }
}