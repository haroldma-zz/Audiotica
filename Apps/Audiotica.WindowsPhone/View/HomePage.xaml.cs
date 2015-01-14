#region

using System;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.ApplicationModel.Store;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Spotify.Models;
using Audiotica.ViewModel;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.View
{
    public sealed partial class HomePage
    {
        public HomePage()
        {
            InitializeComponent();
            Bar = BottomAppBar;
            BottomAppBar = null;
        }

        //TODO [Harry,20140908] move this to view model with RelayCommand
        private async void TopSongsGridView_ItemClick(object sender, ItemClickEventArgs e)
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

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
        }

        private async void MostPlayedGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var song = e.ClickedItem as Song;
            var vm = DataContext as MainViewModel;
            var queueSong = vm.MostPlayed.ToList();
            await CollectionHelper.PlaySongsAsync(song, queueSong);
        }

        private void RecommendationListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var artist = e.ClickedItem as LastArtist;
            App.Navigator.GoTo<SpotifyArtistPage, ZoomInTransition>("name." + artist.Name);
        }

        private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("mailto:help@audiotica.fm"));
        }

        private void AppBarButton_Click_2(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private void SongButton_Click(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<CollectionPage, ZoomInTransition>(0);
        }

        private void ArtistButton_Click(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<CollectionPage, ZoomInTransition>(1);
        }

        private void AlbumButton_Click(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<CollectionPage, ZoomInTransition>(2);
        }

        private void PlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<CollectionPage, ZoomInTransition>(3);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<SearchPage, ZoomInTransition>(null);
        }

        private void SettingsButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<SettingsPage, ZoomOutTransition>(null);
        }
    }
}