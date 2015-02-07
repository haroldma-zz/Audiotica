#region

using GalaSoft.MvvmLight.Messaging;

using IF.Lastfm.Core.Objects;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#endregion

namespace Audiotica.View
{
    public sealed partial class ArtistPage
    {
        public ArtistPage()
        {
            InitializeComponent();
        }

        public override void NavigatedTo(NavigationMode mode, object parameter)
        {
            base.NavigatedTo(mode, parameter);
            var name = parameter as string;

            if (name == null || mode == NavigationMode.Back)
            {
                return;
            }

            var msg = new GenericMessage<string>(name);
            Messenger.Default.Send(msg, "artist-detail-name");
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as LastAlbum;
            if (album != null)
            {
                App.Navigator.GoTo<AlbumPage, ZoomInTransition>(album);
            }
        }
    }
}