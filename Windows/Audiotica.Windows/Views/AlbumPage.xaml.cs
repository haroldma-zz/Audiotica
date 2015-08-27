using Audiotica.Windows.ViewModels;

namespace Audiotica.Windows.Views
{
    public sealed partial class AlbumPage
    {
        public AlbumPage()
        {
            InitializeComponent();
            ViewModel = DataContext as AlbumPageViewModel;
        }

        public AlbumPageViewModel ViewModel { get; }
    }
}