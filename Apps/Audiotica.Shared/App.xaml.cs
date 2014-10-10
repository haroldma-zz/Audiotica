#region

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227
using System;
using System.Threading.Tasks;
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
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.RunTime;
using Audiotica.View;
using Audiotica.ViewModel;
using GalaSoft.MvvmLight.Threading;
using GoogleAnalytics;
using Microsoft.Practices.ServiceLocation;
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
                DispatcherHelper.Initialize();

                Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        await ServiceLocator.Current.GetInstance<ISqlService>().InitializeAsync();
                        await ServiceLocator.Current.GetInstance<ICollectionService>().LoadLibraryAsync();
                        await ServiceLocator.Current.GetInstance<IQueueService>().LoadQueueAsync();
                        ServiceLocator.Current.GetInstance<AudioPlayerManager>().Initialize();
                    }
                    catch (Exception ex)
                    {
                        EasyTracker.GetTracker().SendException(ex.Message + " " + ex.StackTrace, true);
                        DispatcherHelper.RunAsync(() => CurtainPrompt.ShowError("Problem booting app services."));
                    }
                });
                
#if BETA
                await BetaChangelogHelper.OnLaunchedAsync();
#endif
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
            var rootFrame = (Frame)sender;
            rootFrame.ContentTransitions = _transitions ?? new TransitionCollection {new NavigationThemeTransition()};
            rootFrame.Navigated -= RootFrame_FirstNavigated;

            StatusBar.GetForCurrentView().BackgroundOpacity = 1;
            StatusBar.GetForCurrentView().ForegroundColor = Colors.White;
            StatusBar.GetForCurrentView().BackgroundColor = Core.Utilities.ColorHelper.GetColorFromHexa("#4B216D");
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