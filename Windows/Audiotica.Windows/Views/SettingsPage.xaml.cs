using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Audiotica.Core.Windows.Helpers;
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

        private void ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
        {
            if (!DeviceHelper.IsType(DeviceFamily.Mobile)) return;
            var isDark = ThemeSwitch.IsOn;
            StatusBar.GetForCurrentView().ForegroundColor = (isDark ? Colors.White : Colors.Black) as Color?;
        }
    }
}