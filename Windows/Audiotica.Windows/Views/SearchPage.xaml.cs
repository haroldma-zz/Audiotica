using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Audiotica.Windows.ViewModels;

namespace Audiotica.Windows.Views
{
    public sealed partial class SearchPage
    {
        public SearchPage()
        {
            InitializeComponent();
            ViewModel = DataContext as SearchPageViewModel;
        }

        public SearchPageViewModel ViewModel { get; }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ((ComboBox) sender).SelectedIndex = 0;
        }
    }
}