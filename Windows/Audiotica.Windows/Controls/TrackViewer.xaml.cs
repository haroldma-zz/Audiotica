using Windows.UI.Xaml;
using Audiotica.Database.Models;
using Audiotica.Windows.Services;
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

        private void AddButton_Click(object sender,RoutedEventArgs e)
        {

        }
    }
}