using Audiotica.Windows.ViewModels;

namespace Audiotica.Windows.Views
{
    public sealed partial class ManualMatchPage
    {
        public ManualMatchPage()
        {
            InitializeComponent();
            ViewModel = DataContext as ManualMatchPageViewModel;
        }

        public ManualMatchPageViewModel ViewModel { get; }
    }
}