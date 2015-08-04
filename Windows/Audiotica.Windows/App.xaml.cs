using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Audiotica.Core.Windows.Helpers;
using Audiotica.Windows.Views;
using Microsoft.ApplicationInsights;

namespace Audiotica.Windows
{
    sealed partial class App
    {
        public App()
        {
            WindowsAppInitializer.InitializeAsync();
            InitializeComponent();
        }

        public new static App Current => Application.Current as App;

        public override Task OnInitializeAsync()
        {
            // Set the bounds for the view to the core window
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

            // Mobile will be using a different shell
            if (DeviceHelper.IsType(DeviceHelper.Family.Mobile))
                Window.Current.Content = new ShellPhone(RootFrame);
            else
                Window.Current.Content = new Shell(RootFrame);

            return Task.FromResult(0);
        }

        public override Task OnLaunchedAsync(ILaunchActivatedEventArgs e)
        {
            NavigationService.Navigate(typeof (ExplorePage));
            return Task.FromResult(0);
        }
    }
}