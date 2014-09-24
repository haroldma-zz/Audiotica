using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Audiotica.Core;
using Audiotica.Core.Utilities;
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

        private void ArtistListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var artist = e.ClickedItem as Artist;
            Frame.Navigate(typeof(CollectionArtistPage), artist.Id);
        }

        private async void DeleteSongMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var song = ((FrameworkElement) sender).DataContext as Song;

            try
            {
                //delete from the queue
                await App.Locator.QueueService.DeleteAsync(song);

                //stop playback
                if (song.Id == AppSettingsHelper.Read<long>(PlayerConstants.CurrentTrack))
                    App.Locator.AudioPlayer.ShutdownPlayer();

                //wait a bit, there's a chance the player will try to read th db
                await Task.Delay(1000);

                await App.Locator.CollectionService.DeleteSongAsync(song);
                CurtainPrompt.Show("Song deleted");
            }
            catch
            {
                CurtainPrompt.ShowError("Problem deleting song");
            }
        }
    }
}