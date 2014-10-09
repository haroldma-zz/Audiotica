#region

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.View;
using Audiotica.ViewModel;
using GalaSoft.MvvmLight.Threading;
using GoogleAnalytics;
using SlideView.Library;

#endregion

namespace Audiotica
{
    public sealed partial class App : Application
    {
#if WINDOWS_PHONE_APP
        private TransitionCollection _transitions;
#endif

        public static ViewModelLocator Locator
        {
            get { return Current.Resources["Locator"] as ViewModelLocator; }
        }

        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = Resources["SlideApplicationFrame"] as SlideApplicationFrame;

                Window.Current.Content = rootFrame;

                try
                {
                    DispatcherHelper.Initialize();
                    await Locator.SqlService.InitializeAsync();
                    await Locator.CollectionService.LoadLibraryAsync();
                    await Locator.QueueService.LoadQueueAsync();
                    Locator.AudioPlayer.Initialize();
#if BETA
                    await BetaChangelogHelper.OnLaunchedAsync();
#endif
                }
                catch (Exception ex)
                {
                    EasyTracker.GetTracker().SendException(ex.Message + " " + ex.StackTrace, true);
                    CurtainPrompt.ShowError("Problem booting app services.");
                }
            }

            if (rootFrame != null && rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    _transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        _transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += RootFrame_FirstNavigated;
#endif

                if (!rootFrame.Navigate(typeof (HomePage), e.Arguments))
                {
                    CurtainPrompt.ShowError("Failed to create initial page");
                }
            }

            Window.Current.Activate();
        }

#if WINDOWS_PHONE_APP
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = _transitions ?? new TransitionCollection {new NavigationThemeTransition()};
            rootFrame.Navigated -= RootFrame_FirstNavigated;

            //Make sure the statusbar foreground is always black
            StatusBar.GetForCurrentView().ForegroundColor = Colors.White;
        }
#endif

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}