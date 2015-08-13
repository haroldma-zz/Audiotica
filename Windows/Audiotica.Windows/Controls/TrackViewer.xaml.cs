using Windows.UI.Xaml;
using Audiotica.Core.Exceptions;
using Audiotica.Core.Extensions;
using Audiotica.Database.Models;
using Audiotica.Windows.Common;
using Audiotica.Windows.Services;
using Audiotica.Windows.Services.Interfaces;
using Autofac;

namespace Audiotica.Windows.Controls
{
    public sealed partial class TrackViewer
    {
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof (bool), typeof (TrackViewer), null);

        public TrackViewer()
        {
            InitializeComponent();
        }

        public bool IsSelected

        {
            get { return (bool) GetValue(IsSelectedProperty); }

            set { SetValue(IsSelectedProperty, value); }
        }

        public Track Track => DataContext as Track;

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            using (var lifetimeScope = App.Current.Kernel.BeginScope())
            {
                var playerService = lifetimeScope.Resolve<IWindowsPlayerService>();
                playerService.Play(Track);
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            using (var lifetimeScope = App.Current.Kernel.BeginScope())
            {
                var trackSaveService = lifetimeScope.Resolve<ITrackSaveService>();

                try
                {
                    Track.Status = Track.TrackStatus.Saving;
                    Track.SetFrom(await trackSaveService.SaveAsync(Track));
                    CurtainPrompt.Show("Song saved.");
                }
                catch (AppException ex)
                {
                    Track.Status = Track.TrackStatus.None;
                    CurtainPrompt.ShowError(ex.Message ?? "Problem saving song.");
                }
            }
        }
    }
}