#region

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection;
using Audiotica.View;
using Audiotica.ViewModel;
using GalaSoft.MvvmLight.Threading;
using GoogleAnalytics;
using Microsoft.Practices.ServiceLocation;
using SlideView.Library;
using ColorHelper = Audiotica.Core.Utilities.ColorHelper;

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

        public static SlideApplicationFrame RootFrame { get; private set; }

        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            RootFrame = Window.Current.Content as SlideApplicationFrame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (RootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                RootFrame = Resources["SlideApplicationFrame"] as SlideApplicationFrame;

                Window.Current.Content = RootFrame;
                DispatcherHelper.Initialize();

                Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        await ServiceLocator.Current.GetInstance<ISqlService>().InitializeAsync();
                        await ServiceLocator.Current.GetInstance<ICollectionService>().LoadLibraryAsync();
                        await ServiceLocator.Current.GetInstance<IQueueService>().LoadQueueAsync();
                    }
                    catch (Exception ex)
                    {
                        EasyTracker.GetTracker().SendException(ex.Message + " " + ex.StackTrace, true);
                        DispatcherHelper.RunAsync(() => CurtainPrompt.ShowError("ErrorBootingToast".FromLanguageResource()));
                    }
                });

#if BETA
                await BetaChangelogHelper.OnLaunchedAsync();
#endif
            }

            if (RootFrame != null && RootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (RootFrame.ContentTransitions != null)
                {
                    _transitions = new TransitionCollection();
                    foreach (var c in RootFrame.ContentTransitions)
                    {
                        _transitions.Add(c);
                    }
                }

                RootFrame.ContentTransitions = null;
                RootFrame.Navigated += RootFrame_FirstNavigated;
#endif

                if (!RootFrame.Navigate(typeof (HomePage), e.Arguments))
                {
                    CurtainPrompt.ShowError("Failed to create initial page");
                }
            }

            Locator.AudioPlayerHelper.OnAppActive();
            Window.Current.Activate();
        }

#if WINDOWS_PHONE_APP
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            RootFrame.ContentTransitions = _transitions ?? new TransitionCollection {new NavigationThemeTransition()};
            RootFrame.Navigated -= RootFrame_FirstNavigated;

            StatusBar.GetForCurrentView().BackgroundOpacity = 1;
            StatusBar.GetForCurrentView().ForegroundColor = Colors.White;
            StatusBar.GetForCurrentView().BackgroundColor = ColorHelper.GetColorFromHexa("#4B216D");
        }
#endif

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            Locator.AudioPlayerHelper.OnAppSuspended();

            deferral.Complete();
        }
    }
}