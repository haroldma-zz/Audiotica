using Windows.UI.Xaml;
using Audiotica.Web.Models;

namespace Audiotica.Windows.Controls
{
    public sealed partial class MatchViewer
    {
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof (bool), typeof (MatchViewer), null);

        private MatchSong _match;

        public MatchViewer()
        {
            InitializeComponent();
        }

        public bool IsSelected

        {
            get { return (bool) GetValue(IsSelectedProperty); }

            set { SetValue(IsSelectedProperty, value); }
        }

        public MatchSong Match
        {
            get { return _match; }
            set
            {
                _match = value;
                Bindings.Update();
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}