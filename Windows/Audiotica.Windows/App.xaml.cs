using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Store;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Core.Windows.Helpers;
using Audiotica.Windows.Common;
using Audiotica.Windows.Views;
using Microsoft.HockeyApp;

namespace Audiotica.Windows
{
    sealed partial class App
    {
        public App()
        {
            InitializeComponent();

            HockeyClient.Current.Configure("c6b42065168f44dab41240ac167fda8d",
                new TelemetryConfiguration
                {
                    Collectors =
                        WindowsCollectors.Metadata | WindowsCollectors.Session | WindowsCollectors.UnhandledException | WindowsCollectors.PageView | WindowsCollectors.WatsonData
                });
#if DEBUG
            LicenseInformation = CurrentAppSimulator.LicenseInformation;
#else
            LicenseInformation = CurrentApp.LicenseInformation;
#endif
        }

        public LicenseInformation LicenseInformation { get; }

        public static new App Current => Application.Current as App;

        public Shell Shell { get; private set; }

        // runs even if restored from state
        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            // Wrap the frame in the shell (hamburger menu)
            Shell = new Shell();
            Window.Current.Content = Shell;

            if (DeviceHelper.IsType(DeviceFamily.Mobile))
            {
                var appSettings = Kernel.Resolve<IAppSettingsUtility>();
                var isDark = appSettings.Theme == 2 || appSettings.Theme == 0;
                StatusBar.GetForCurrentView().ForegroundColor = isDark ? Colors.White : Colors.Black;
                StatusBar.GetForCurrentView().BackgroundColor = isDark ? Colors.Black : Colors.White;
                StatusBar.GetForCurrentView().BackgroundOpacity = 1;
            }

            await Task.CompletedTask;
        }

        // runs only when not restored from state
        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            NavigationService.Navigate(typeof(AlbumsPage));
            await Task.CompletedTask;
        }

        protected override bool OnUnhandledException(Exception ex)
        {
            CurtainPrompt.ShowError("Crash prevented",
                () =>
                    {
                        MessageBox.Show(
                            "The problem has been reported.  If you continue to experience this bug, email support.  Details: "
                                +
                                ex.Message,
                            "Crash prevented");
                    });
            return true;
        }
    }
}