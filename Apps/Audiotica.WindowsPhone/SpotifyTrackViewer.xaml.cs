using Audiotica.Data.Spotify.Models;

using Windows.UI.Xaml;

namespace Audiotica
{
    public sealed partial class SpotifyTrackViewer
    {
        public SpotifyTrackViewer()
        {
            this.InitializeComponent();
        }

        private async void MenuFlyoutItemClick(object sender, RoutedEventArgs e)
        {
            await CollectionHelper.SaveTrackAsync(this.DataContext as FullTrack, true);
        }
    }
}