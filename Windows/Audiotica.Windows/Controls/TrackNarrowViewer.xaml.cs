using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Audiotica.Core.Exceptions;
using Audiotica.Database.Models;
using Audiotica.Windows.Common;
using Audiotica.Windows.Services.Interfaces;
using Autofac;

namespace Audiotica.Windows.Controls
{
    // TODO: find a way to get state triggers to work on usercontrol, then we won't need a seperate control _sight_ (hopefully just a bug on the current SDK)
    public sealed partial class TrackNarrowViewer
    {
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof (bool), typeof (TrackNarrowViewer), null);

        public static readonly DependencyProperty IsCatalogProperty =
           DependencyProperty.Register("IsCatalog", typeof(bool), typeof(TrackViewer), null);

        private Track _track;

        public TrackNarrowViewer()
        {
            InitializeComponent();
        }

        public bool IsSelected

        {
            get { return (bool) GetValue(IsSelectedProperty); }

            set { SetValue(IsSelectedProperty, value); }
        }

        public bool IsCatalog

        {
            get { return (bool)GetValue(IsCatalogProperty); }

            set { SetValue(IsCatalogProperty, value); }
        }

        public Track Track
        {
            get { return _track; }
            set
            {
                _track = value;
                Bindings.Update();
            }
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            using (var lifetimeScope = App.Current.Kernel.BeginScope())
            {
                var playerService = lifetimeScope.Resolve<IPlayerService>();
                try
                {
                    var queue = await playerService.AddAsync(Track);
                    // player auto plays when there is only one track
                    if (playerService.PlaybackQueue.Count > 1)
                        playerService.Play(queue);
                }
                catch (AppException ex)
                {
                    CurtainPrompt.ShowError(ex.Message ?? "Something happened.");
                }
            }
        }

        private async void AddCollection_Click(object sender, RoutedEventArgs e)
        {
            var button = (MenuFlyoutItem)sender;
            button.IsEnabled = false;

            using (var scope = App.Current.Kernel.BeginScope())
            {
                var trackSaveService = scope.Resolve<ITrackSaveService>();

                try
                {
                    await trackSaveService.SaveAsync(Track);
                }
                catch (AppException ex)
                {
                    Track.Status = TrackStatus.None;
                    CurtainPrompt.ShowError(ex.Message ?? "Problem saving song.");
                }
                finally
                {
                    button.IsEnabled = true;
                }
            }
        }

        private async void AddQueue_Click(object sender, RoutedEventArgs e)
        {
            using (var scope = App.Current.Kernel.BeginScope())
            {
                var backgroundAudioService = scope.Resolve<IPlayerService>();
                try
                {
                    await backgroundAudioService.AddAsync(Track);
                    CurtainPrompt.Show("Added to queue");
                }
                catch (AppException ex)
                {
                    CurtainPrompt.ShowError(ex.Message ?? "Something happened.");
                }
            }
        }

        private async void AddUpNext_Click(object sender, RoutedEventArgs e)
        {
            using (var scope = App.Current.Kernel.BeginScope())
            {
                var backgroundAudioService = scope.Resolve<IPlayerService>();
                try
                {
                    await backgroundAudioService.AddUpNextAsync(Track);
                    CurtainPrompt.Show("Added up next");
                }
                catch (AppException ex)
                {
                    CurtainPrompt.ShowError(ex.Message ?? "Something happened.");
                }
            }
        }
    }
}