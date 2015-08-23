using Windows.UI.Xaml.Navigation;
using Audiotica.Windows.ViewModels;

namespace Audiotica.Windows.Views
{
    public sealed partial class ArtistsPage
    {
        public ArtistsPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            ViewModel = DataContext as ArtistsPageViewModel;
        }

        public ArtistsPageViewModel ViewModel { get; }
    }
}