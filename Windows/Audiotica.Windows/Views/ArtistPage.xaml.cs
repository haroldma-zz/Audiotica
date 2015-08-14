using Audiotica.Windows.ViewModels;

namespace Audiotica.Windows.Views
{
    public sealed partial class ArtistPage
    {
        public ArtistPage()
        {
            InitializeComponent();
            ViewModel = DataContext as ArtistPageViewModel;
        }

        public ArtistPageViewModel ViewModel { get; set; }
    }
}