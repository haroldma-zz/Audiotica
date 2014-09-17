#region

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Navigation;
using Audiotica.Collection.Model;
using Audiotica.ViewModel;
using GalaSoft.MvvmLight.Messaging;

#endregion

namespace Audiotica.View
{
    public sealed partial class AlbumPage
    {
        public AlbumPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var id = e.Parameter as string;

            if (id == null) return;

            var msg = new GenericMessage<string>(id);
            Messenger.Default.Send(msg, "album-detail-id");
        }

        private void HyperlinkButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var vm = (sender as HyperlinkButton).DataContext as AlbumViewModel;

            Frame.Navigate(typeof (ArtistPage), vm.Album.PrimaryArtist.Id);
        }
    }
}