using Windows.UI.Xaml.Navigation;
using Audiotica.Windows.ViewModels;

namespace Audiotica.Windows.Views
{
    public sealed partial class SongsPage
    {
        public SongsPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            ViewModel = DataContext as SongsPageViewModel;
        }

        public SongsPageViewModel ViewModel { get; set; }
    }
}