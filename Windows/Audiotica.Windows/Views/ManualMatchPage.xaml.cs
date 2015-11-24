using System;
using Windows.Media.Playback;
using Audiotica.Web.Models;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.ViewModels;

namespace Audiotica.Windows.Views
{
    public sealed partial class ManualMatchPage
    {
        public ManualMatchPage()
        {
            InitializeComponent();
            ViewModel = DataContext as ManualMatchPageViewModel;
        }

        public ManualMatchPageViewModel ViewModel { get; }

        private void MatchViewer_OnPlayClick(object sender, MatchSong e)
        {
            var player = App.Current.Kernel.Resolve<IPlayerService>();
            if (player.CurrentState == MediaPlayerState.Playing)
                player.PlayOrPause();

            PlaybackPlayer.Source = new Uri(e.AudioUrl);
            PlaybackPlayer.Play();
        }
    }
}