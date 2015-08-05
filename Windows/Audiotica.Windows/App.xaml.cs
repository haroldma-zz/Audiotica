using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Audiotica.Core.Windows.Helpers;
using Microsoft.ApplicationInsights;

namespace Audiotica.Windows
{
    sealed partial class App
    {
        public App()
        {
            WindowsAppInitializer.InitializeAsync();

            // Only portrait is supported on mobile
            if (DeviceHelper.IsType(DeviceHelper.Family.Mobile))
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            // Only the dark theme is supported in everything else (they only have light option)
            else
                RequestedTheme = ApplicationTheme.Dark;

            InitializeComponent();
        }

        public new static App Current => Application.Current as App;

        public override Task OnInitializeAsync()
        {
            // Set the bounds for the view to the core window
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

            // Wrap the frame in the shell (hamburger menu)
            Window.Current.Content = new Shell(RootFrame);

            return Task.FromResult(0);
        }

        public override Task OnLaunchedAsync(ILaunchActivatedEventArgs e)
        {
            NavigationService.Navigate(NavigationService.DefaultPage);
            return Task.FromResult(0);
        }
    }
}