using Audiotica.Windows.ViewModels;

namespace Audiotica.Windows.Views
{
    public sealed partial class ArtistsPage
    {
        public ArtistsPage()
        {
            InitializeComponent();
            ViewModel = DataContext as ArtistsPageViewModel;
        }

        public ArtistsPageViewModel ViewModel { get; }
    }
}