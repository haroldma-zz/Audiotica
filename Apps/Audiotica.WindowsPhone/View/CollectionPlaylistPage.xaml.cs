#region

using Windows.UI.Xaml.Navigation;
using GalaSoft.MvvmLight.Messaging;

#endregion

namespace Audiotica.View
{
    public sealed partial class CollectionPlaylistPage
    {
        public CollectionPlaylistPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var id = e.Parameter as long?;

            if (id == null) return;

            var msg = new GenericMessage<long>((long) id);
            Messenger.Default.Send(msg, "playlist-coll-detail-id");
        }
    }
}