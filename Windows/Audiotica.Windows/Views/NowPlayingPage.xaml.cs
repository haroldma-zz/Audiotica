using Audiotica.Windows.ViewModels;

namespace Audiotica.Windows.Views
{
    public sealed partial class NowPlayingPage
    {
        public NowPlayingPage()
        {
            InitializeComponent();
            ViewModel = DataContext as NowPlayingPageViewModel;
        }

        public NowPlayingPageViewModel ViewModel { get; }
    }
}