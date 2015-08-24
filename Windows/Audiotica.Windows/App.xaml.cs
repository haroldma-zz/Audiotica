using Windows.ApplicationModel.Activation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Audiotica.Core.Windows.Helpers;
using Microsoft.ApplicationInsights;

namespace Audiotica.Windows
{
    sealed partial class App
    {
        public App()
        {
            WindowsAppInitializer.InitializeAsync();

            // Only the dark theme is supported in everything else (they only have light option)
            if (!DeviceHelper.IsType(DeviceFamily.Mobile))
                RequestedTheme = ApplicationTheme.Dark;

            InitializeComponent();
        }

        public new static App Current => Application.Current as App;

        public override void OnInitialize()
        {
            // Set the bounds for the view to the core window
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

            // Wrap the frame in the shell (hamburger menu)
            Window.Current.Content = new Shell(RootFrame);
        }

        public override void OnLaunched(ILaunchActivatedEventArgs e)
        {
            NavigationService.Navigate(NavigationService.DefaultPage);
        }
    }
}