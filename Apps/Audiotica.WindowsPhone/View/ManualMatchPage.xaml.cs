using System;

using Windows.System;

using Audiotica.Data.Collection.Model;
using Audiotica.Data.Model;
using Audiotica.ViewModel;

using GalaSoft.MvvmLight.Messaging;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Audiotica.View
{
    public sealed partial class ManualMatchPage
    {
        public ManualMatchPage()
        {
            InitializeComponent();
        }

        public override void NavigatedTo(NavigationMode mode, object parameter)
        {
            base.NavigatedTo(mode, parameter);
            var song = parameter as Song;
            Messenger.Default.Send(song, "manual-match");
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as WebSong;

            if (item.IsLinkDeath)
            {
                return;
            }

            Launcher.LaunchUriAsync(new Uri(item.AudioUrl));
           
        }

        private void AppBarButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var item = (sender as AppBarButton).DataContext as WebSong;

            if (item.IsLinkDeath)
            {
                return;
            }

            var vm = DataContext as ManualMatchViewModel;

            vm.CurrentSong.AudioUrl = item.AudioUrl;
            vm.CurrentSong.SongState = SongState.None;
            App.Locator.SqlService.UpdateItemAsync(vm.CurrentSong);
            App.Navigator.GoBack();
        }
    }
}