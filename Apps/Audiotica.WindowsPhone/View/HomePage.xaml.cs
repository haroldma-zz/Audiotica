#region

using System;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Audiotica.Core.WinRt.Common;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Model;
using Audiotica.Data.Spotify.Models;
using Audiotica.View.Setting;
using Audiotica.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.View
{
    public sealed partial class HomePage
    {
        private readonly HubSection _spotlightSection;
        private readonly int _spotlightIndex;

        public HomePage()
        {
            InitializeComponent();
            Bar = BottomAppBar;
            BottomAppBar = null;
            _spotlightSection = SpotlightSection;
            _spotlightIndex = MainHub.Sections.IndexOf(SpotlightSection);
            MainHub.Sections.Remove(_spotlightSection);
            Messenger.Default.Register<bool>(this, "spotlight", SpotlightLoaded);
        }

        private void SpotlightLoaded(bool loaded)
        {
            if (loaded)
            {
                MainHub.Sections.Insert(_spotlightIndex, _spotlightSection);
            }
        }

        private void HandleAction(SpotlightFeature feature)
        {
            var action = feature.Action.Substring(feature.Action.IndexOf(":", StringComparison.Ordinal) + 1);
            var supported = true;

            if (feature.Action.StartsWith("web:"))
            {
                Launcher.LaunchUriAsync(new Uri(action));
            }
            else if (feature.Action.StartsWith("artist:"))
            {
                App.Navigator.GoTo<SpotifyArtistPage, ZoomInTransition>("name." + action);
            }
            else if (feature.Action.StartsWith("page:"))
            {
                switch (action)
                {
                    case "cloud":
                        App.Navigator.GoTo<CloudPage, ZoomOutTransition>(null);
                        break;
                    case "lastfm":
                        App.Navigator.GoTo<LastFmPage, ZoomOutTransition>(null);
                        break;
                    default:
                        if (action.StartsWith("search:"))
                        {
                            action = action.Substring(feature.Action.IndexOf(":", StringComparison.Ordinal) + 1);
                            App.Navigator.GoTo<SearchPage, ZoomInTransition>(action);
                        }
                        else
                        {
                            supported = false;
                        }
                        break;
                }
            }
            else
            {
                supported = false;
            }

            if (!supported)
            {
                CurtainPrompt.ShowError("Audiotica can't open this type of link.  Try updating in the store.");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<SearchPage, ZoomInTransition>(null);
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<SettingsPage, ZoomOutTransition>(null);
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

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var feature = (SpotlightFeature)e.ClickedItem;
            HandleAction(feature);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var feature = (SpotlightFeature)((FrameworkElement)sender).DataContext;
            HandleAction(feature);
        }

        private async void MostPlayedGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var song = e.ClickedItem as Song;
            var vm = DataContext as MainViewModel;
            var queueSong = vm.MostPlayed.ToList();
            await CollectionHelper.PlaySongsAsync(song, queueSong);
        }

        private async void TopSongsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var chartTrack = e.ClickedItem as ChartTrack;
            if (chartTrack == null) return;

            await CollectionHelper.SaveTrackAsync(chartTrack);
        }

        private void RecommendationListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var artist = e.ClickedItem as LastArtist;
            App.Navigator.GoTo<SpotifyArtistPage, ZoomInTransition>("name." + artist.Name);
        }

        private async void RecentlyAddedGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var song = e.ClickedItem as Song;
            var vm = DataContext as MainViewModel;
            var queueSong = vm.RecentlyAdded.ToList();
            await CollectionHelper.PlaySongsAsync(song, queueSong);
        }

        private async void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            var queueSong = App.Locator.CollectionService.Songs.ToList();
            await CollectionHelper.PlaySongsAsync(queueSong, true);
        }
    }
}