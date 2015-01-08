#region

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Store;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.View;
using Audiotica.ViewModel;
using GalaSoft.MvvmLight.Threading;
using GoogleAnalytics;

#endregion

namespace Audiotica
{
    public sealed partial class App
    {
        public static bool IsDebugging = Debugger.IsAttached;

        #region Fields

#if WINDOWS_PHONE_APP
        private readonly ContinuationManager _continuationManager;
#endif
        private bool _init;
        private static ViewModelLocator _locator;

        #endregion

        #region Properties

        public static ViewModelLocator Locator
        {
            get { return _locator ?? (_locator = Current.Resources["Locator"] as ViewModelLocator); }
        }

        public static Frame RootFrame { get; private set; }

        public static LicenseInformation LicenseInformation
        {
            get
            {
                return IsDebugging
                ? CurrentApp.LicenseInformation
                : CurrentAppSimulator.LicenseInformation;
            }
        }

        #endregion

        #region Constructor

        public App()
        {
            InitializeComponent();
            _continuationManager = new ContinuationManager();
            Suspending += OnSuspending;
            Resuming += OnResuming;
            UnhandledException += OnUnhandledException;
            AppVersionHelper.OnLaunched();
            EasyTracker.GetTracker().AppVersion =
                AppVersionHelper.CurrentVersion + (IsDebugging ? "" : "-beta");

            Current.DebugSettings.EnableFrameRateCounter = AppSettingsHelper.Read<bool>("FrameRateCounter");
            Current.DebugSettings.EnableRedrawRegions = AppSettingsHelper.Read<bool>("RedrawRegions");
        }

        #endregion

        #region Overriding

        protected override async void OnActivated(IActivatedEventArgs e)
        {
            base.OnActivated(e);

            CreateRootFrame();

            if (RootFrame.Content == null)
            {
                RootFrame.Navigated += RootFrame_FirstNavigated;
                RootFrame.Navigate(typeof (HomePage));
            }

            var continuationEventArgs = e as IContinuationActivatedEventArgs;

            if (continuationEventArgs != null)
            {
                _continuationManager.Continue(continuationEventArgs, RootFrame);
            }

            Window.Current.Activate();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            CreateRootFrame();

            var restore = await StorageHelper.FileExistsAsync("_current_restore.autcp");

            var page = typeof (HomePage);

            if (AppVersionHelper.IsFirstRun)
                page = typeof (FirstRunPage);
            else if (restore)
                page = typeof (RestorePage);

            if (RootFrame.Content == null)
            {
                RootFrame.Navigated += RootFrame_FirstNavigated;

                //MainPage is always in rootFrame so we don't have to worry about restoring the navigation state on resume
                RootFrame.Navigate(page, e.Arguments);
            }

            // Ensure the current window is active
            Window.Current.Activate();

            // ReSharper disable once CSharpWarnings::CS4014
            if (!restore)
                BootAppServicesAsync();

            var dataManager = DataTransferManager.GetForCurrentView();
            dataManager.DataRequested += DataTransferManagerOnDataRequested;
        }

        #endregion

        #region Events

        private async void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            RootFrame.Navigated -= RootFrame_FirstNavigated;
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            ApplicationView.GetForCurrentView().VisibleBoundsChanged += OnVisibleBoundsChanged;
            OnVisibleBoundsChanged(null, null);

            await ReviewReminderAsync();

            #region On update

            if (!AppVersionHelper.JustUpdated) return;

            CurtainPrompt.Show(2500, "AppUpdated".FromLanguageResource(), AppVersionHelper.CurrentVersion);

            //download missing artwork for artist
            if (Locator.CollectionService.IsLibraryLoaded)
                await CollectionHelper.DownloadArtistsArtworkAsync();
            else
                Locator.CollectionService.LibraryLoaded +=
                    async (o, args) => await CollectionHelper.DownloadArtistsArtworkAsync();

            #endregion
        }

        private void OnVisibleBoundsChanged(ApplicationView sender, object args)
        {
            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            var h = Window.Current.Bounds.Height;

            var diff = Math.Ceiling(h - bounds.Bottom);
            RootFrame.Margin = new Thickness(0, 0, 0, diff);
        }

        private void OnResuming(object sender, object o)
        {
            Locator.AudioPlayerHelper.OnAppActive();
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            Locator.AudioPlayerHelper.OnAppSuspended();
            deferral.Complete();
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (AppSettingsHelper.Read("Crashing", true)) return;

            e.Handled = true;
            MessageBox.Show(e.Message + "\n" + e.Exception.StackTrace, "[DEV-MODE] Crashing Error");
        }

        private void DataTransferManagerOnDataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            var request = e.Request;
            request.Data.Properties.Title = "Checkout Audiotica!";
            request.Data.Properties.Description = "A sleek and sexy music app for Windows Phone!";

            const string url = "http://www.windowsphone.com/s?appid=c02aae72-99d3-480f-b6d2-3fac2fed08a7";
            request.Data.SetText(request.Data.Properties.Description + " " + url);
        }

        #endregion

        private async Task RestoreStatusAsync(ApplicationExecutionState previousExecutionState)
        {
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (previousExecutionState == ApplicationExecutionState.Terminated)
            {
                // Restore the saved session state only when appropriate
                try
                {
                    await SuspensionManager.RestoreAsync();
                }
                catch (SuspensionManagerException)
                {
                    //Something went wrong restoring state.
                    //Assume there is no state and continue
                }
            }
        }

        private void CreateRootFrame()
        {
            RootFrame = Window.Current.Content as Frame;

            if (RootFrame == null)
            {
                RootFrame = new Frame {Style = (Style) Resources["AppFrame"]};

                Window.Current.Content = RootFrame;
                DispatcherHelper.Initialize();
            }
        }

        public async Task BootAppServicesAsync()
        {
            if (!_init)
            {
                try
                {
                    await Locator.SqlService.InitializeAsync().ConfigureAwait(false);
                    await Locator.BgSqlService.InitializeAsync().ConfigureAwait(false);
                    await Locator.CollectionService.LoadLibraryAsync().ConfigureAwait(false);
                    DispatcherHelper.RunAsync(() => Locator.Download.LoadDownloads());
                }
                catch (Exception ex)
                {
                    EasyTracker.GetTracker().SendException(ex.Message + " " + ex.StackTrace, true);
                    DispatcherHelper.RunAsync(() => CurtainPrompt.ShowError("AppErrorBooting".FromLanguageResource()));
                }

                _init = true;
            }
            Locator.AudioPlayerHelper.OnAppActive();
        }

        private async Task ReviewReminderAsync()
        {
            var launchCount = AppSettingsHelper.Read<int>("LaunchCount");
            AppSettingsHelper.Write("LaunchCount", ++launchCount);
            if (launchCount != 5) return;

            var rate = "FeedbackDialogRateButton".FromLanguageResource();

            var md = new MessageDialog(
                "FeedbackDialogContent".FromLanguageResource(),
                "FeedbackDialogTitle".FromLanguageResource());
            md.Commands.Add(new UICommand(rate));
            md.Commands.Add(new UICommand("FeedbackDialogNoButton".FromLanguageResource()));

            var result = await md.ShowAsync();

            if (result.Label == rate)
            {
                Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
            }
        }
    }
}