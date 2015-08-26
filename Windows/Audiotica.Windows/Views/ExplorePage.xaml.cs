using Windows.UI.Xaml.Navigation;
using Audiotica.Windows.ViewModels;

namespace Audiotica.Windows.Views
{
    public sealed partial class ExplorePage
    {
        public ExplorePage()
        {
            InitializeComponent();
            ViewModel = DataContext as ExplorePageViewModel;
        }

        public ExplorePageViewModel ViewModel { get; set; }
    }
}