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

            // Set the bounds for the view to the core window
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

            if (DeviceHelper.IsType(DeviceFamily.Mobile))
            {
                ApplicationView.GetForCurrentView().VisibleBoundsChanged += OnVisibleBoundsChanged;
                OnVisibleBoundsChanged(null, null);

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

        private void OnVisibleBoundsChanged(ApplicationView sender, object args)
        {
            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            var h = Window.Current.Bounds.Height;
            var w = Window.Current.Bounds.Width;

            var top = Math.Ceiling(bounds.Top + h - h);
            var left = Math.Ceiling(bounds.Left + w - w);
            var right = Math.Ceiling(w - bounds.Right);
            var bottom = Math.Ceiling(h - bounds.Bottom);
            RootFrame.Margin = new Thickness(left, 0, right, bottom);
            Shell.Padding = new Thickness(left, top, 0, 0);
        }
    }
}