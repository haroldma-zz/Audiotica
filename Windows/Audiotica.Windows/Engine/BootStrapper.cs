using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Metadata;
using Windows.Globalization;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Audiotica.Windows.Engine.Navigation;

namespace Audiotica.Windows.Engine
{
    public abstract class BootStrapper : Application
    {
        public const string DefaultTileId = "App";
        private const string CacheDateKey = "Setting-Cache-Date";

        private object _pageKeys;

        protected BootStrapper()
        {
            Current = this;
            UnhandledException += App_UnhandledException;
            Resuming += (s, o) =>
                {
                    Kernel.OnResuming();
                    OnResuming(s, o);
                };
            Suspending += async (s, e) =>
                {
                    // one, global deferral
                    var deferral = e.SuspendingOperation.GetDeferral();
                    try
                    {
                        foreach (var service in WindowWrapper.ActiveWrappers.SelectMany(x => x.NavigationServices))
                        {
                            // date the cache (which marks the date/time it was suspended)
                            service.FrameFacade.SetFrameState(CacheDateKey,
                                DateTime.Now.ToString(CultureInfo.InvariantCulture));
                            // call view model suspend (OnNavigatedfrom)
                            await service.SuspendingAsync();
                        }
                        Kernel.OnSuspending();
                        // call system-level suspend
                        await OnSuspendingAsync(s, e);
                    }
                    catch
                    {
                        // ignored
                    }
                    finally
                    {
                        deferral.Complete();
                    }
                };
        }

        // this event precedes the in-frame event by the same name
        public static event EventHandler<HandledEventArgs> BackRequested;

        // this event precedes the in-frame event by the same name
        public static event EventHandler<HandledEventArgs> ForwardRequested;

        public event EventHandler<WindowCreatedEventArgs> WindowCreated;

        public enum AdditionalKinds
        {
            Primary,
            Toast,
            SecondaryTile,
            Other,
            JumpListItem
        }

        public enum BackButton
        {
            Attach,
            Ignore
        }

        public enum ExistingContent
        {
            Include,
            Exclude
        }

        public enum StartKind
        {
            Launch,
            Activate
        }

        public static new BootStrapper Current { get; private set; }

        /// <summary>
        ///     CacheMaxDuration indicates the maximum TimeSpan for which cache data
        ///     will be preserved. If Template 10 determines cache data is older than
        ///     the specified MaxDuration it will automatically be cleared during start.
        /// </summary>
        public TimeSpan CacheMaxDuration { get; set; } = TimeSpan.MaxValue;

        public bool ForceShowShellBackButton { get; set; } = false;

        public AppKernel Kernel { get; private set; }

        public INavigationService NavigationService => WindowWrapper.Current().NavigationServices.First();

        public Frame RootFrame { get; set; } = new Frame();

        public StateItems SessionState { get; set; } = new StateItems();

        /// <summary>
        ///     ShowShellBackButton is used to show or hide the shell-drawn back button that
        ///     is new to Windows 10. A developer can do this manually, but using this property
        ///     is important during navigation because Template 10 manages the visibility
        ///     of the shell-drawn back button at that time.
        /// </summary>
        public bool ShowShellBackButton { get; set; } = true;

        /// <summary>
        ///     The SplashFactory is a Func that returns an instantiated Splash view.
        ///     Template 10 will automatically inject this visual before loading the app.
        /// </summary>
        protected Func<SplashScreen, UserControl> SplashFactory { get; set; }

        /// <summary>
        ///     This determines the simplest case for starting. This should handle 80% of common scenarios.
        ///     When Other is returned the developer must determine start manually using IActivatedEventArgs.Kind
        /// </summary>
        public static AdditionalKinds DetermineStartCause(IActivatedEventArgs args)
        {
            if (args is ToastNotificationActivatedEventArgs)
            {
                return AdditionalKinds.Toast;
            }
            var e = args as ILaunchActivatedEventArgs;
            if (e?.TileId == DefaultTileId && string.IsNullOrEmpty(e?.Arguments))
            {
                return AdditionalKinds.Primary;
            }
            if (e?.TileId == DefaultTileId && !string.IsNullOrEmpty(e?.Arguments))
            {
                return AdditionalKinds.JumpListItem;
            }
            if (e?.TileId != null && e?.TileId != DefaultTileId)
            {
                return AdditionalKinds.SecondaryTile;
            }
            return AdditionalKinds.Other;
        }

        /// <summary>
        ///     Creates a new NavigationService from the gived Frame to the
        ///     WindowWrapper collection. In addition, it optionally will setup the
        ///     shell back button to react to the nav of the Frame.
        ///     A developer should call this when creating a new/secondary frame.
        ///     The shell back button should only be setup one time.
        /// </summary>
        public INavigationService NavigationServiceSetup(
            BackButton backButton,
            ExistingContent existingContent,
            Frame frame)
        {
            frame.Language = ApplicationLanguages.Languages[0];
            frame.Content = existingContent == ExistingContent.Include ? Window.Current.Content : null;

            var navigationService = new NavigationService(frame);
            navigationService.FrameFacade.BackButtonHandling = backButton;
            WindowWrapper.Current().NavigationServices.Add(navigationService);

            if (backButton == BackButton.Attach)
            {
                // TODO: unattach others

                // update shell back when backstack changes
                // only the default frame in this case because secondary should not dismiss the app
                frame.RegisterPropertyChangedCallback(Frame.BackStackDepthProperty, (s, args) => UpdateShellBackButton());

                // update shell back when navigation occurs
                // only the default frame in this case because secondary should not dismiss the app
                frame.Navigated += (s, args) => UpdateShellBackButton();
            }

            // this is always okay to check, default or not
            // expire any state (based on expiry)
            DateTime cacheDate;
            // default the cache age to very fresh if not known
            var otherwise = DateTime.MinValue.ToString(CultureInfo.InvariantCulture);
            if (DateTime.TryParse(navigationService.FrameFacade.GetFrameState(CacheDateKey, otherwise), out cacheDate))
            {
                var cacheAge = DateTime.Now.Subtract(cacheDate);
                if (cacheAge >= CacheMaxDuration)
                {
                    // clear state in every nav service in every view
                    foreach (var service in WindowWrapper.ActiveWrappers.SelectMany(x => x.NavigationServices))
                    {
                        service.FrameFacade.ClearFrameState();
                    }
                }
            }

            return navigationService;
        }

        /// <summary>
        ///     OnInitializeAsync is where your app will do must-have up-front operations
        ///     OnInitializeAsync will be called even if the application is restoring from state.
        ///     An app restores from state when the app was suspended and then terminated (PreviousExecutionState terminated).
        /// </summary>
        public virtual async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            await Task.CompletedTask;
        }

        public virtual void OnResuming(object s, object e)
        {
        }

        /// <summary>
        ///     OnStartAsync is the one-stop-show override to handle when your app starts
        ///     Template 10 will not call OnStartAsync if the app is restored from state.
        ///     An app restores from state when the app was suspended and then terminated (PreviousExecutionState terminated).
        /// </summary>
        public abstract Task OnStartAsync(StartKind startKind, IActivatedEventArgs args);

        /// <summary>
        ///     OnSuspendingAsync will be called when the application is suspending, but this override
        ///     should only be used by applications that have application-level operations that must
        ///     be completed during suspension.
        ///     Using OnSuspendingAsync is a little better than handling the Suspending event manually
        ///     because the asunc operations are in a single, global deferral created when the suspension
        ///     begins and completed automatically when the last viewmodel has been called (including this method).
        /// </summary>
        public virtual async Task OnSuspendingAsync(object s, SuspendingEventArgs e)
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile"))
            {
                WindowWrapper.ClearNavigationServices(Window.Current);
            }
            await Task.CompletedTask;
        }

        // T must be a custom Enum
        public Dictionary<T, Type> PageKeys<T>()
            where T : struct, IConvertible
        {
            if (!typeof (T).GetTypeInfo().IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }
            var keys = _pageKeys as Dictionary<T, Type>;
            if (keys != null)
            {
                return keys;
            }
            return (Dictionary<T, Type>)(_pageKeys = new Dictionary<T, Type>());
        }

        public virtual T Resolve<T>(Type type) => default(T);

        public virtual INavigable ResolveForPage(Type page, NavigationService navigationService) => null;

        public void UpdateShellBackButton()
        {
            // show the shell back only if there is anywhere to go in the default frame
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                ShowShellBackButton && (NavigationService.Frame.CanGoBack || ForceShowShellBackButton)
                    ? AppViewBackButtonVisibility.Visible
                    : AppViewBackButtonVisibility.Collapsed;
        }

        // it is the intent of Template 10 to no longer require Launched/Activated overrides, only OnStartAsync()

        protected override sealed async void OnActivated(IActivatedEventArgs e)
        {
            await InternalActivatedAsync(e);
        }

        protected override sealed async void OnCachedFileUpdaterActivated(CachedFileUpdaterActivatedEventArgs args)
        {
            await InternalActivatedAsync(args);
        }

        protected override sealed async void OnFileActivated(FileActivatedEventArgs args)
        {
            await InternalActivatedAsync(args);
        }

        protected override sealed async void OnFileOpenPickerActivated(FileOpenPickerActivatedEventArgs args)
        {
            await InternalActivatedAsync(args);
        }

        protected override sealed async void OnFileSavePickerActivated(FileSavePickerActivatedEventArgs args)
        {
            await InternalActivatedAsync(args);
        }

        // it is the intent of Template 10 to no longer require Launched/Activated overrides, only OnStartAsync()

        protected override sealed void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (Kernel == null)
            {
                Kernel = AppKernelFactory.Create();
            }

            InternalLaunchAsync(e);
        }

        protected override sealed async void OnSearchActivated(SearchActivatedEventArgs args)
        {
            await InternalActivatedAsync(args);
        }

        protected override sealed async void OnShareTargetActivated(ShareTargetActivatedEventArgs args)
        {
            await InternalActivatedAsync(args);
        }

        protected virtual bool OnUnhandledException(Exception ex)
        {
            return false;
        }

        protected override sealed void OnWindowCreated(WindowCreatedEventArgs args)
        {
            var window = new WindowWrapper(args.Window);
            WindowCreated?.Invoke(this, args);
            base.OnWindowCreated(args);
        }

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

        /// <summary>
        ///     InitializeFrameAsync creates a default Frame preceeded by the optional
        ///     splash screen, then OnInitialzieAsync, then the new frame (if necessary).
        ///     This is private because there's no reason for the developer to call this.
        /// </summary>
        private async Task InitializeAsync(IActivatedEventArgs e)
        {
            // first show the splash 
            FrameworkElement splash = null;
            if (SplashFactory != null)
            {
                Window.Current.Content = splash = SplashFactory(e.SplashScreen);
                Window.Current.Activate();
            }

            // allow the user to do things, even when restoring
            await OnInitializeAsync(e);

            // setup custom titlebar
            /*foreach (var resource in Application.Current.Resources
                .Where(x => x.Key.Equals(typeof (CustomTitleBar))))
            {
                var control = new CustomTitleBar { Style = resource.Value as Style };
            }*/

            InternalNavigationServiceSetup(BackButton.Attach, ExistingContent.Include);

            // create the default frame only if there's nothing already there
            // if it is not null, by the way, then the developer injected something & they win
            if (Window.Current.Content == null || Window.Current.Content == splash)
            {
                Window.Current.Content = RootFrame;
            }
        }

        /// <summary>
        ///     This handles all the prelimimary stuff unique to Activated before calling OnStartAsync()
        ///     This is private because it is a specialized prelude to OnStartAsync().
        ///     OnStartAsync will not be called if state restore is determined.
        /// </summary>
        private async Task InternalActivatedAsync(IActivatedEventArgs e)
        {
            // sometimes activate requires a frame to be built
            if (Window.Current.Content == null)
            {
                await InitializeAsync(e);
            }

            // onstart is shared with activate and launch
            await OnStartAsync(StartKind.Activate, e);

            // ensure active (this will hide any custom splashscreen)
            Window.Current.Activate();
        }

        /// <summary>
        ///     This handles all the preliminary stuff unique to Launched before calling OnStartAsync().
        ///     This is private because it is a specialized prelude to OnStartAsync().
        ///     OnStartAsync will not be called if state restore is determined
        /// </summary>
        private async void InternalLaunchAsync(ILaunchActivatedEventArgs e)
        {
            if (e.PreviousExecutionState != ApplicationExecutionState.Running)
            {
                await InitializeAsync(e);
            }

            if (e.PreviousExecutionState != ApplicationExecutionState.Terminated)
            {
                Kernel.OnLaunched();
            }

            // okay, now handle launch
            switch (e.PreviousExecutionState)
            {
                //case ApplicationExecutionState.ClosedByUser:
                case ApplicationExecutionState.Terminated:
                    {
                        /*
                            Restore state if you need to/can do.
                            Remember that only the primary tile should restore.
                            (this includes toast with no data payload)
                            The rest are already providing a nav path.

                            In the event that the cache has expired, attempting to restore
                            from state will fail because of missing values. 
                            This is okay & by design.
                        */

                        if (DetermineStartCause(e) == AdditionalKinds.Primary)
                        {
                            var restored = NavigationService.RestoreSavedNavigation();
                            Kernel.OnRelaunched();
                            if (!restored)
                            {
                                await OnStartAsync(StartKind.Launch, e);
                            }
                        }
                        else
                        {
                            Kernel.OnLaunched();
                            await OnStartAsync(StartKind.Launch, e);
                        }

                        SubscribeBackButton();

                        break;
                    }
                case ApplicationExecutionState.ClosedByUser:
                case ApplicationExecutionState.NotRunning:
                    // launch if not restored
                    await OnStartAsync(StartKind.Launch, e);

                    SubscribeBackButton();

                    break;
                default:
                    {
                        // launch if not restored
                        await OnStartAsync(StartKind.Launch, e);
                        break;
                    }
            }

            // ensure active (this will hide any custom splashscreen)
            Window.Current.Activate();

            // Hook up keyboard and mouse Back handler
            var keyboard = new KeyboardService.KeyboardService();
            keyboard.AfterBackGesture = () =>
                {
                    //the result is no matter
                    var handled = false;
                    RaiseBackRequested(ref handled);
                };

            // Hook up keyboard and mouse Forward handler
            keyboard.AfterForwardGesture = RaiseForwardRequested;
        }

        /// <summary>
        ///     Creates a new NavigationService from the gived Frame to the
        ///     WindowWrapper collection. In addition, it optionally will setup the
        ///     shell back button to react to the nav of the Frame.
        ///     A developer should call this when creating a new/secondary frame.
        ///     The shell back button should only be setup one time.
        /// </summary>
        private void InternalNavigationServiceSetup(
            BackButton backButton,
            ExistingContent existingContent)
        {
            RootFrame.Language = ApplicationLanguages.Languages[0];
            RootFrame.Content = existingContent == ExistingContent.Include ? Window.Current.Content : null;

            var navigationService = Kernel.Resolve<INavigationService>();
            navigationService.FrameFacade.BackButtonHandling = backButton;
            WindowWrapper.Current().NavigationServices.Add(navigationService);

            if (backButton == BackButton.Attach)
            {
                // TODO: unattach others

                // update shell back when backstack changes
                // only the default frame in this case because secondary should not dismiss the app
                RootFrame.RegisterPropertyChangedCallback(Frame.BackStackDepthProperty,
                    (s, args) => UpdateShellBackButton());

                // update shell back when navigation occurs
                // only the default frame in this case because secondary should not dismiss the app
                RootFrame.Navigated += (s, args) => UpdateShellBackButton();
            }

            // this is always okay to check, default or not
            // expire any state (based on expiry)
            DateTime cacheDate;
            // default the cache age to very fresh if not known
            var otherwise = DateTime.MinValue.ToString(CultureInfo.InvariantCulture);
            if (DateTime.TryParse(navigationService.FrameFacade.GetFrameState(CacheDateKey, otherwise), out cacheDate))
            {
                var cacheAge = DateTime.Now.Subtract(cacheDate);
                if (cacheAge >= CacheMaxDuration)
                {
                    // clear state in every nav service in every view
                    foreach (var service in WindowWrapper.ActiveWrappers.SelectMany(x => x.NavigationServices))
                    {
                        service.FrameFacade.ClearFrameState();
                    }
                }
            }
        }

        /// <summary>
        ///     Default Hardware/Shell Back handler overrides standard Back behavior
        ///     that navigates to previous app in the app stack to instead cause a backward page navigation.
        ///     Views or Viewodels can override this behavior by handling the BackRequested
        ///     event and setting the Handled property of the BackRequestedEventArgs to true.
        /// </summary>
        private void RaiseBackRequested(ref bool handled)
        {
            var args = new HandledEventArgs();
            BackRequested?.Invoke(null, args);
            if (handled = args.Handled)
            {
                return;
            }
            foreach (var frame in WindowWrapper.Current().NavigationServices.Select(x => x.FrameFacade).Reverse())
            {
                frame.RaiseBackRequested(args);
                if (handled = args.Handled)
                {
                    return;
                }
            }
            NavigationService.GoBack();
        }

        private void RaiseForwardRequested()
        {
            var args = new HandledEventArgs();
            ForwardRequested?.Invoke(null, args);
            if (args.Handled)
            {
                return;
            }
            foreach (var frame in WindowWrapper.Current().NavigationServices.Select(x => x.FrameFacade))
            {
                frame.RaiseForwardRequested(args);
                if (args.Handled)
                {
                    return;
                }
            }
            NavigationService.GoForward();
        }

        private void SubscribeBackButton()
        {
            // Hook up the default Back handler
            SystemNavigationManager.GetForCurrentView().BackRequested += (s, args) =>
                {
                    var handled = false;
                    if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0))
                    {
                        if (NavigationService.CanGoBack)
                        {
                            handled = true;
                        }
                    }
                    else
                    {
                        handled = !NavigationService.CanGoBack;
                    }

                    RaiseBackRequested(ref handled);
                    args.Handled = handled;
                };
        }
    }
}