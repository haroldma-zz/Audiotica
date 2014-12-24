#region

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection.Model;

#endregion


namespace Audiotica.View
{
    public sealed partial class NowPlayingPage
    {
        public NowPlayingPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var song = App.Locator.Player.CurrentQueue;
            App.Locator.Player.CurrentQueue = null;
            App.Locator.Player.CurrentQueue = song;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            var song = App.Locator.Player.CurrentQueue;
            App.Locator.Player.CurrentQueue = null;
            App.Locator.Player.CurrentQueue = song; 
        }

        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            var queue = e.AddedItems[0] as QueueSong;

            //make sure on same page
            if (Frame.CurrentSourcePageType != typeof (NowPlayingPage)) return;

            var vm = DataContext as PlayerViewModel;
            if (queue != null && vm.CurrentQueue.Id != queue.Id)
            {
                vm.AudioPlayerHelper.PlaySong(queue);
            }
        }
    }
}