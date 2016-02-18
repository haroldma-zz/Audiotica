using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Core.Windows.Helpers;
using Audiotica.Windows.Common;
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
            }

            await Task.CompletedTask;
        }

        // runs only when not restored from state
        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            NavigationService.Navigate(typeof (AlbumsPage));
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