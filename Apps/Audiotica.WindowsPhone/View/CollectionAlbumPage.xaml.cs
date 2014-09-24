using Windows.UI.Xaml.Navigation;
using GalaSoft.MvvmLight.Messaging;

namespace Audiotica.View
{
    public sealed partial class CollectionAlbumPage
    {
        public CollectionAlbumPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var id = e.Parameter as long?;

            if (id == null) return;

            var msg = new GenericMessage<long>((long)id);
            Messenger.Default.Send(msg, "album-coll-detail-id");
        }
    }
}