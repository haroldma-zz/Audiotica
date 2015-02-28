#region

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.View
{
    public sealed partial class AlbumPage
    {
        public AlbumPage()
        {
            InitializeComponent();
        }

        public override void NavigatedTo(NavigationMode mode, object parameter)
        {
            base.NavigatedTo(mode, parameter);

            var album = parameter as LastAlbum;

            if (album == null || mode == NavigationMode.Back) return;

            var msg = new GenericMessage<LastAlbum>(album);
            Messenger.Default.Send(msg, "album-detail");
        }

        public override void NavigatedFrom(NavigationMode mode)
        {
            base.NavigatedFrom(mode);

            if (mode != NavigationMode.Back) return;

            var vm = DataContext as AlbumViewModel;
            vm.Album = null;
            vm.Tracks = null;
        }
    }
}