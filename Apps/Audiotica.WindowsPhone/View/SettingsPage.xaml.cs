#region

using Windows.UI.Xaml;
using Audiotica.View.Setting;

#endregion

namespace Audiotica.View
{
    public sealed partial class SettingsPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void ApplicationButton_OnClick(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<ApplicationPage, ZoomInTransition>(null);
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<PlayerPage, ZoomInTransition>(null);
        }

        private void DeveloperButton_OnClick(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<DeveloperPage, ZoomInTransition>(null);
        }

        private void LastFmButton_OnClick(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<LastFmPage, ZoomInTransition>(null);
        }

        private void AboutButton_OnClick(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<AboutPage, ZoomInTransition>(null);
        }

        private void CloudButton_OnClick(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<CloudPage, ZoomInTransition>(null);
        }

        private void CollectionButton_OnClick(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<Setting.CollectionSettingsPage, ZoomInTransition>(null);
        }

        private void StatisticsButton_OnClick(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<CollectionStatisticsPage, ZoomInTransition>(null);
        }
    }
}