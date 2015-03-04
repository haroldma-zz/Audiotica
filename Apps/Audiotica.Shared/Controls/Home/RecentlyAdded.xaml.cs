using Windows.UI.Xaml.Controls;
using Audiotica.Data.Collection.Model;
using Audiotica.ViewModel;

namespace Audiotica.Controls.Home
{
    public sealed partial class RecentlyAdded
    {
        public RecentlyAdded()
        {
            InitializeComponent();
        }

        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var song = e.ClickedItem as Song;
            var vm = DataContext as RecentlyAddedViewModel;
            var queueSong = vm.RecentlyAdded;
            await CollectionHelper.PlaySongsAsync(song, queueSong);
        }
    }
}