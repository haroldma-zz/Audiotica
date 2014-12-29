#region

using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Spotify.Models;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.View
{
    public sealed partial class HomePage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        //TODO [Harry,20140908] move this to view model with RelayCommand
        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var chartTrack = e.ClickedItem as ChartTrack;
            if (chartTrack == null) return;

            await CollectionHelper.SaveTrackAsync(chartTrack);
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var artist = ((Grid) sender).DataContext as LastArtist;
            Frame.Navigate(typeof (SpotifyArtistPage), "name." + artist.Name);
        }
    }
}