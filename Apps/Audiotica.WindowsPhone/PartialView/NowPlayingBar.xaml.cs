#region

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Audiotica.Core;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.RunTime;
using Audiotica.View;

#endregion

namespace Audiotica.PartialView
{
    public sealed partial class NowPlayingBar
    {
        public NowPlayingBar()
        {
            InitializeComponent();

            var visBinding = new Binding {Source = DataContext, Path = new PropertyPath("CurrentQueue")};
            SetBinding(IsVisibleProperty, visBinding);

            (App.Locator.CollectionService as CollectionService).PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "CurrentPlaybackQueue")
            {
                SongFlipView.SelectedItem = App.Locator.Player.CurrentQueue;
            }
        }

        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.RegisterAttached("IsVisible", typeof(object), typeof(NowPlayingBar),
                new PropertyMetadata(null, IsVisibleCallback));

        public static void SetIsVisible(DependencyObject element, object value)
        {
            element.SetValue(IsVisibleProperty, value);
        }

        public static object GetIsVisible(DependencyObject element)
        {
            return element.GetValue(IsVisibleProperty);
        }

        private static void IsVisibleCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var state = e.NewValue != null ? "Visible" : "Collapsed";
            var nowPlayingBar = d as NowPlayingBar;

            if (e.NewValue != null)
            {
                var containsSong = true;
                while (containsSong)
                {
                    containsSong = !App.Locator.CollectionService.PlaybackQueue.Contains(e.NewValue);

                    if (!containsSong)
                    {
                        nowPlayingBar.SongFlipView.SelectedItem = e.NewValue;
                    }
                }
            }

            VisualStateManager.GoToState(nowPlayingBar, state, true);
        }

        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var song = SongFlipView.SelectedItem as QueueSong;

            if (song == null) return;

            var currentPlayingId = AppSettingsHelper.Read<long>(PlayerConstants.CurrentTrack);

            if (e.RemovedItems.Count != 0 && song.Id != currentPlayingId)
            {
                App.Locator.AudioPlayerHelper.PlaySong(song);
            }
        }

        private void SongFlipView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NowPlayingSheetUtility.OpenNowPlaying();
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            SlideDown.Completed += SlideDownOnCompleted;
            SlideDown.Begin();
        }

        private void SlideDownOnCompleted(object sender, object o)
        {
            SlideDown.Completed -= SlideDownOnCompleted;
            NowPlayingSheetUtility.OpenNowPlaying();
        }
    }
}