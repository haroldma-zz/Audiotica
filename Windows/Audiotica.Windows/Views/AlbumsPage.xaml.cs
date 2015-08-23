using Windows.UI.Xaml.Navigation;
using Audiotica.Windows.ViewModels;

namespace Audiotica.Windows.Views
{
    public sealed partial class AlbumsPage
    {
        public AlbumsPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            ViewModel = DataContext as AlbumsPageViewModel;
        }

        public AlbumsPageViewModel ViewModel { get; }
    }
}