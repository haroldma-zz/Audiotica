#region

using System;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Service.RunTime;

#endregion

namespace Audiotica.PartialView
{
    public sealed partial class NowPlayingSheet : IModalSheetPage
    {
        public NowPlayingSheet()
        {
            InitializeComponent();
            CurrentQueueView.Loaded += CurrentQueueViewOnLoaded;
            _lyrcis = new LyricsService();
        }

        private void CurrentQueueViewOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            CurrentQueueView.SelectedItem = App.Locator.Player.CurrentQueue;
            CurrentQueueView.ScrollIntoView(CurrentQueueView.SelectedItem);
        }

        public Popup Popup { get; private set; }
        private bool _justLoaded;
        public void OnOpened(Popup popup)
        {
            _justLoaded = true;
            Popup = popup;
            App.Locator.AudioPlayerHelper.TrackChanged += AudioPlayerHelperOnTrackChanged;
        }

        private void AudioPlayerHelperOnTrackChanged(object sender, EventArgs eventArgs)
        {
            CurrentQueueView.SelectedItem = App.Locator.Player.CurrentQueue;
            CurrentQueueView.ScrollIntoView(CurrentQueueView.SelectedItem);
            Pivot_SelectionChanged(null, null);
        }

        public void OnClosed()
        {
            CurrentQueueView.Loaded -= CurrentQueueViewOnLoaded;
            App.Locator.AudioPlayerHelper.TrackChanged -= AudioPlayerHelperOnTrackChanged;
            DataContext = null;
            Popup = null;
        }

        private void CurrentQueueView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var song = CurrentQueueView.SelectedItem as QueueSong;

            if (song == null) return;

            var currentPlayingId = App.Locator.AppSettingsHelper.Read<int>(PlayerConstants.CurrentTrack);

            if (e.RemovedItems.Count != 0 && song.Id != currentPlayingId)
            {
                App.Locator.AudioPlayerHelper.PlaySong(song);
            }
        }

        private bool _seeking;
        private TypedEventHandler<ListViewBase, ContainerContentChangingEventArgs> _delegate;
        private readonly LyricsService _lyrcis;

        private void Slider_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            _seeking = true;
            BackgroundMediaPlayer.Current.Pause();
        }

        private void Slider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _seeking = false;
            BackgroundMediaPlayer.Current.Position = App.Locator.Player.Position;
            BackgroundMediaPlayer.Current.Play();
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (DataContext == null || _justLoaded || App.Locator.Player.IsLoading 
                || BackgroundMediaPlayer.Current.NaturalDuration.TotalSeconds == 0)
            {
                _justLoaded = false;
                return;
            }

            var diff = App.Locator.Player.Position.TotalSeconds - BackgroundMediaPlayer.Current.Position.TotalSeconds;
            if (!(diff > 10) && !(diff < -10)) return;

            if (_seeking) return;

            BackgroundMediaPlayer.Current.Position = App.Locator.Player.Position;
            BackgroundMediaPlayer.Current.Play();
        }

        private void ItemListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var songViewer = args.ItemContainer.ContentTemplateRoot as SongViewer;

            if (songViewer == null)
                return;

            if (args.InRecycleQueue)
            {
                songViewer.ClearData();
            }
            else switch (args.Phase)
                {
                    case 0:
                        songViewer.ShowPlaceholder((args.Item as QueueSong).Song, true);
                        args.RegisterUpdateCallback(ContainerContentChangingDelegate);
                        break;
                    case 1:
                        songViewer.ShowTitle();
                        args.RegisterUpdateCallback(ContainerContentChangingDelegate);
                        break;
                    case 2:
                        songViewer.ShowRest();
                        break;
                }

            // For imporved performance, set Handled to true since app is visualizing the data item 
            args.Handled = true;
        }

        /// <summary>
        ///     Managing delegate creation to ensure we instantiate a single instance for
        ///     optimal performance.
        /// </summary>
        private TypedEventHandler<ListViewBase, ContainerContentChangingEventArgs> ContainerContentChangingDelegate
        {
            get { return _delegate ?? (_delegate = ItemListView_ContainerContentChanging); }
        }

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Pivot.SelectedIndex != 2) return;

            var song = App.Locator.Player.CurrentQueue.Song;
            if (LyricsTextBlock.Tag as int? == song.Id) return;

            LyricsTextBlock.Tag = song.Id;
            LyricsTextBlock.Text = "Loading...";

            var results = await _lyrcis.GetLyrics(song.Name, song.Artist.Name);

            LyricsTextBlock.Text = string.IsNullOrEmpty(results) ? "No lyrics found for this song." : results;
        }
    }
}