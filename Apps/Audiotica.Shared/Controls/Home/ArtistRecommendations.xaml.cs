using Windows.UI.Xaml.Controls;
using Audiotica.View;
using IF.Lastfm.Core.Objects;

namespace Audiotica.Controls.Home
{
    public sealed partial class ArtistRecommendations
    {
        public ArtistRecommendations()
        {
            InitializeComponent();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var artist = e.ClickedItem as LastArtist;
            App.Navigator.GoTo<SpotifyArtistPage, ZoomInTransition>("name." + artist.Name);
        }
    }
}