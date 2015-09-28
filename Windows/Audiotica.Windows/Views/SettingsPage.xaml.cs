using Audiotica.Windows.ViewModels;

namespace Audiotica.Windows.Views
{
    public sealed partial class SettingsPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            ViewModel = DataContext as SettingsPageViewModel;
        }

        public SettingsPageViewModel ViewModel { get; }
    }
}