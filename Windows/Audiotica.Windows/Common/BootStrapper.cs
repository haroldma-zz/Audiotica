using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Globalization;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Audiotica.Core.Helpers;
using Audiotica.Windows.AppEngine;
using Audiotica.Windows.Factories;
using Audiotica.Windows.Services.NavigationService;

namespace Audiotica.Windows.Common
{
    // BootStrapper is a drop-in replacement of Application
    // - OnInitializeAsync is the first in the pipeline, if launching
    // - OnLaunchedAsync is required, and second in the pipeline
    // - OnActivatedAsync is first in the pipeline, if activating
    // - NavigationService is an automatic property of this class
    public abstract class BootStrapper : Application
    {
        protected BootStrapper()
        {
            Resuming += (s, e) =>
            {
                Kernel.OnResuming();
                OnResuming(s, e);
            };
            Suspending += (s, e) =>
            {
                var deferral = e.SuspendingOperation.GetDeferral();
                NavigationService.Suspending();
                Kernel.OnSuspending();
                OnSuspending(s, e);
                deferral.Complete();
            };
            UnhandledException += App_UnhandledException;
        }

        /// <summary>
        ///     Event to allow views and viewmodels to intercept the Hardware/Shell Back request and
        ///     implement their own logic, such as closing a dialog. In your event handler, set the
        ///     Handled property of the BackRequestedEventArgs to true if you do not want a Page
        ///     Back navigation to occur.
        /// </summary>
        public event EventHandler<BackRequestedEventArgs> BackRequested;

        protected sealed override void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (Kernel == null)
                Kernel = AppKernelFactory.Create();
            
            InternalLaunch(e);
        }

        private void InternalLaunch(ILaunchActivatedEventArgs e)
        {
            var splashScreen = default(UIElement);
            if (SplashFactory != null)
            {
                splashScreen = SplashFactory(e.SplashScreen);
                Window.Current.Content = splashScreen;
            }

            RootFrame = RootFrame ?? new Frame();
            RootFrame.Language = ApplicationLanguages.Languages[0];
            NavigationService = Kernel.Resolve<INavigationService>();

            // the user may override to set custom content
            OnInitialize();

            if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                Kernel.OnRelaunched();
                try
                {
                    NavigationService.RestoreSavedNavigation();
                }
                finally
                {
                    OnRelaunched(e);
                }
            }
            else
            {
                Kernel.OnLaunched();
                OnLaunched(e);
            }

            // if the user didn't already set custom content use rootframe
            if (Window.Current.Content == splashScreen || Window.Current.Content == null)
            {
                Window.Current.Content = RootFrame;
            }
            Window.Current.Activate();

            // Hook up the default Back handler
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }

        /// <summary>
        ///     Default Hardware/Shell Back handler overrides standard Back behavior that navigates to previous app
        ///     in the app stack to instead cause a backward page navigation.
        ///     Views or Viewodels can override this behavior by handling the BackRequested event and setting the
        ///     Handled property of the BackRequestedEventArgs to true.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            BackRequested?.Invoke(this, e);
            if (e.Handled) return;
            if (!RootFrame.CanGoBack) return;
            RootFrame.GoBack();
            e.Handled = true;
        }

        #region handling

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e != null)
            {
                // handling ad network exceptions
                var exception = e.Exception;
                if ((exception is XmlException || exception is NullReferenceException) &&
                    exception.ToString().ToUpper().Contains("INNERACTIVE"))
                {
                    Debug.WriteLine("Handled Inneractive exception {0}", exception);
                    e.Handled = true;
                    return;
                }
                if (exception is NullReferenceException && exception.ToString().ToUpper().Contains("SOMA"))
                {
                    Debug.WriteLine("Handled Smaato null reference exception {0}", exception);
                    e.Handled = true;
                    return;
                }
                if ((exception is IOException || exception is NullReferenceException) &&
                    exception.ToString().ToUpper().Contains("GOOGLE"))
                {
                    Debug.WriteLine("Handled Google exception {0}", exception);
                    e.Handled = true;
                    return;
                }
                if (exception is ObjectDisposedException && exception.ToString().ToUpper().Contains("MOBFOX"))
                {
                    Debug.WriteLine("Handled Mobfox exception {0}", exception);
                    e.Handled = true;
                    return;
                }
                if ((exception is NullReferenceException || exception is XamlParseException) &&
                    exception.ToString().ToUpper().Contains("MICROSOFT.ADVERTISING"))
                {
                    Debug.WriteLine("Handled Microsoft.Advertising exception {0}", exception);
                    e.Handled = true;
                    return;
                }

                e.Handled = OnUnhandledException(e.Exception);
            }
        }

        #endregion

        #region properties

        public AppKernel Kernel { get; set; }
        public Frame RootFrame { get; set; }
        public INavigationService NavigationService { get; private set; }
        protected Func<SplashScreen, Page> SplashFactory { get; set; }

        #endregion

        #region activated

        protected override void OnActivated(IActivatedEventArgs e)
        {
            InternalActivated(e);
        }

        protected override void OnCachedFileUpdaterActivated(CachedFileUpdaterActivatedEventArgs args)
        {
            InternalActivated(args);
        }

        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            InternalActivated(args);
        }

        protected override void OnFileOpenPickerActivated(FileOpenPickerActivatedEventArgs args)
        {
            InternalActivated(args);
        }

        protected override void OnFileSavePickerActivated(FileSavePickerActivatedEventArgs args)
        {
            InternalActivated(args);
        }

        protected override void OnSearchActivated(SearchActivatedEventArgs args)
        {
            InternalActivated(args);
        }

        protected override void OnShareTargetActivated(ShareTargetActivatedEventArgs args)
        {
            InternalActivated(args);
        }

        private void InternalActivated(IActivatedEventArgs e)
        {
            OnActivated(e);
            Window.Current.Activate();
        }

        #endregion

        #region overrides

        public virtual void OnInitialize()
        {
        }

        public abstract void OnLaunched(ILaunchActivatedEventArgs e);

        public virtual void OnRelaunched(ILaunchActivatedEventArgs e)
        {
        }

        protected virtual void OnResuming(object s, object e)
        {
        }

        protected virtual void OnSuspending(object s, SuspendingEventArgs e)
        {
        }

        protected virtual bool OnUnhandledException(Exception ex)
        {
            return false;
        }

        #endregion
    }
}