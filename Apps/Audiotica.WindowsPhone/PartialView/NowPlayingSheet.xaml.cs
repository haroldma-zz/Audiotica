#region

using System;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection.Model;

#endregion

namespace Audiotica.PartialView
{
    public sealed partial class NowPlayingSheet : IModalSheetPage
    {
        public NowPlayingSheet()
        {
            InitializeComponent();
            CurrentQueueView.Loaded += CurrentQueueViewOnLoaded;
        }

        private void CurrentQueueViewOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            CurrentQueueView.SelectedItem = App.Locator.Player.CurrentQueue;
            CurrentQueueView.ScrollIntoView(CurrentQueueView.SelectedItem);
        }

        public Popup Popup { get; private set; }
        public void OnOpened(Popup popup)
        {
            Popup = popup;
            App.Locator.AudioPlayerHelper.TrackChanged += AudioPlayerHelperOnTrackChanged;
        }

        private void AudioPlayerHelperOnTrackChanged(object sender, EventArgs eventArgs)
        {
            CurrentQueueView.SelectedItem = App.Locator.Player.CurrentQueue;
            CurrentQueueView.ScrollIntoView(CurrentQueueView.SelectedItem);
        }

        public void OnClosed()
        {
            CurrentQueueView.Loaded -= CurrentQueueViewOnLoaded;
            App.Locator.AudioPlayerHelper.TrackChanged -= AudioPlayerHelperOnTrackChanged;
            Popup = null;
        }

        private void CurrentQueueView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var song = CurrentQueueView.SelectedItem as QueueSong;

            if (song == null) return;

            var currentPlayingId = AppSettingsHelper.Read<long>(PlayerConstants.CurrentTrack);

            if (e.RemovedItems.Count != 0 && song.Id != currentPlayingId)
            {
                App.Locator.AudioPlayerHelper.PlaySong(song);
            }
        }

        private void Slider_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            BackgroundMediaPlayer.Current.Pause();
        }

        private void Slider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            BackgroundMediaPlayer.Current.Position = App.Locator.Player.Position;
            BackgroundMediaPlayer.Current.Play();
        }
    }
}