using Windows.UI.Xaml.Controls;
using Audiotica.Data.Collection.Model;
using Audiotica.ViewModel;

namespace Audiotica.Controls.Home
{
    public sealed partial class MostPlayed
    {
        public MostPlayed()
        {
            InitializeComponent();
        }

        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var song = e.ClickedItem as Song;
            var vm = DataContext as MostPlayedViewModel;
            var queueSong = vm.MostPlayed;
            await CollectionHelper.PlaySongsAsync(song, queueSong);
        }
    }
}