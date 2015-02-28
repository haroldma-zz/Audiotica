#region

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.ViewModel;
using GalaSoft.MvvmLight.Messaging;

#endregion

namespace Audiotica.View
{
    public sealed partial class SpotifyAlbumPage
    {
        public SpotifyAlbumPage()
        {
            InitializeComponent();
        }

        public override void NavigatedTo(NavigationMode mode, object parameter)
        {
            base.NavigatedTo(mode, parameter);
            var album = parameter as string;

            if (album == null) return;

            var msg = new GenericMessage<string>(album);
            Messenger.Default.Send(msg, "spotify-album-detail");
        }

        public override void NavigatedFrom(NavigationMode mode)
        {
            base.NavigatedFrom(mode);
            if (mode != NavigationMode.Back) return;

            var vm = DataContext as SpotifyAlbumViewModel;
            vm.Album = null;
            vm.Tracks = null;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = (sender as Button).DataContext as SpotifyAlbumViewModel;
            await CollectionHelper.SaveAlbumAsync(vm.Album);
        }
    }
}