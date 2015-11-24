using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Audiotica.Web.Models;

namespace Audiotica.Windows.Controls
{
    public sealed partial class MatchViewer
    {
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof (bool), typeof (MatchViewer), null);

        public static readonly DependencyProperty MatchSelectedCommandProperty =
           DependencyProperty.Register("MatchSelectedCommand", typeof(ICommand), typeof(MatchViewer), null);

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

        public ICommand MatchSelectedCommand

        {
            get { return (ICommand) GetValue(MatchSelectedCommandProperty); }

            set { SetValue(MatchSelectedCommandProperty, value); }
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

        private void MatchButton_Click(object sender, object e)
        {
            MatchSelectedCommand?.Execute(Match);
        }
    }
}