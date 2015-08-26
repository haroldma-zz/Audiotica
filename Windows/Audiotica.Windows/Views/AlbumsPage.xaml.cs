using Audiotica.Windows.ViewModels;

namespace Audiotica.Windows.Views
{
    public sealed partial class AlbumsPage
    {
        public AlbumsPage()
        {
            InitializeComponent();
            ViewModel = DataContext as AlbumsPageViewModel;
        }

        public AlbumsPageViewModel ViewModel { get; }
    }
}