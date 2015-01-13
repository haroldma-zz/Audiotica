#region

using GalaSoft.MvvmLight.Messaging;

#endregion

namespace Audiotica.View
{
    public sealed partial class CollectionAlbumPage
    {
        public CollectionAlbumPage()
        {
            InitializeComponent();
        }

        public override void NavigatedTo(object e)
        {
            base.NavigatedTo(e);
            var id = e as int?;

            if (id == null) return;

            var msg = new GenericMessage<int>((int)id);
            Messenger.Default.Send(msg, "album-coll-detail-id");
        }
    }
}