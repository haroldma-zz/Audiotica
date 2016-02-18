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
            StatusBar.GetForCurrentView().ForegroundColor = isDark ? Colors.White : Colors.Black;
            StatusBar.GetForCurrentView().BackgroundColor = isDark ? Colors.Black : Colors.White;
            StatusBar.GetForCurrentView().BackgroundOpacity = 1;
        }

        private void AdsSwitch_Toggled(object sender, RoutedEventArgs e)
        {
           /* if (AdsSwitch.IsOn)
            {
                if (!App.Current.Shell.AdsLoaded)
                    App.Current.Shell.ConfigureAds();
            }
            else if (App.Current.Shell.AdsLoaded)
                App.Current.Shell.DisableAds();*/
        }
    }
}