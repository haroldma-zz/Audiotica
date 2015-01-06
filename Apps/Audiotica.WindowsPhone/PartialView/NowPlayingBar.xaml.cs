#region

using System;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Audiotica.Core;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection.Model;
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
                    
                    if(!containsSong)
                        nowPlayingBar.FlipView.SelectedItem = e.NewValue;
                }
            }

            VisualStateManager.GoToState(nowPlayingBar, state, true);
        }

        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var song = FlipView.SelectedItem as QueueSong;

            if (song == null) return;

            var currentPlayingId = AppSettingsHelper.Read<long>(PlayerConstants.CurrentTrack);

            if (e.RemovedItems.Count != 0 && song.Id != currentPlayingId)
            {
                App.Locator.AudioPlayerHelper.PlaySong(song);
            }
        }
    }
}