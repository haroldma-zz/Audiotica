#region

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Store;
using Windows.Phone.UI.Input;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Service.RunTime;
using Audiotica.View;
using Audiotica.ViewModel;
using GalaSoft.MvvmLight.Threading;
using GoogleAnalytics;
using MyToolkit.Paging.Handlers;

#endregion

namespace Audiotica
{
    public sealed partial class App
    {
        public static bool IsDebugging = Debugger.IsAttached;
        public static bool IsProduction = true;

        #region Fields

#if WINDOWS_PHONE_APP
        private readonly ContinuationManager _continuationManager;
#endif
        private bool _init;
        private static ViewModelLocator _locator;

        #endregion

        #region Properties

        public static Navigator Navigator { get; set; }

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
                ? CurrentAppSimulator.LicenseInformation
                : CurrentApp.LicenseInformation;
            }
        }

        #endregion

        #region Constructor

        public App()
        {
            InitializeComponent();
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
            _continuationManager = new ContinuationManager();
            Suspending += OnSuspending;
            Resuming += OnResuming;
            UnhandledException += OnUnhandledException;
            AppVersionHelper.OnLaunched();
            EasyTracker.GetTracker().AppVersion =
                AppVersionHelper.CurrentVersion + (IsProduction ? "" : "-beta");

            Current.DebugSettings.EnableFrameRateCounter = AppSettingsHelper.Read<bool>("FrameRateCounter");
            Current.DebugSettings.EnableRedrawRegions = AppSettingsHelper.Read<bool>("RedrawRegions");
        }

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs e)
        {
            if (UiBlockerUtility.IsBlocking)
            {
                e.Handled = true;
            }
            else if (Navigator.GoBack())
            {
                e.Handled = true;
            }
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
                RootFrame.Navigate(typeof (RootPage));
            }

            var continuationEventArgs = e as IContinuationActivatedEventArgs;

            if (continuationEventArgs != null)
            {
                _continuationManager.Continue(continuationEventArgs);
            }

            Window.Current.Activate();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            CreateRootFrame();

            var restore = await StorageHelper.FileExistsAsync("_current_restore.autcp");

            if (RootFrame.Content == null)
            {
                RootFrame.Navigated += RootFrame_FirstNavigated;

                //MainPage is always in rootFrame so we don't have to worry about restoring the navigation state on resume
                RootFrame.Navigate(typeof (RootPage), e.Arguments);
            }

            if (e.Arguments.StartsWith("artists/"))
            {
                NowPlayingSheetUtility.CloseNowPlaying();

                if (Navigator.CurrentPage is CollectionArtistPage)
                    Navigator.GoBack();

                var id = int.Parse(e.Arguments.Replace("artists/", ""));

                if (Locator.CollectionService.Artists.FirstOrDefault(p => p.Id == id) != null)
                    Navigator.GoTo<CollectionArtistPage, ZoomInTransition>(id);
                else if (!Locator.CollectionService.IsLibraryLoaded)
                {
                    UiBlockerUtility.Block("Loading collection...");
                    Locator.CollectionService.LibraryLoaded += (sender, args) =>
                    {
                        UiBlockerUtility.Unblock();
                        Navigator.GoTo<CollectionArtistPage,ZoomInTransition>(id);
                    };
                }
            }

            else if (e.Arguments.StartsWith("albums/"))
            {
                NowPlayingSheetUtility.CloseNowPlaying();

                if (Navigator.CurrentPage is CollectionAlbumPage)
                    Navigator.GoBack();

                var id = int.Parse(e.Arguments.Replace("albums/", ""));

                if (Locator.CollectionService.Albums.FirstOrDefault(p => p.Id == id) != null)
                    Navigator.GoTo<CollectionAlbumPage, ZoomInTransition>(id);
                else if (!Locator.CollectionService.IsLibraryLoaded)
                {
                    UiBlockerUtility.Block("Loading collection...");
                    Locator.CollectionService.LibraryLoaded += (sender, args) =>
                    {
                        UiBlockerUtility.Unblock();
                        Navigator.GoTo<CollectionAlbumPage, ZoomInTransition>(id);
                    };
                }
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

            var crash = AppSettingsHelper.ReadJsonAs<Exception>("CrashingException");
            if (crash != null)
                await WarnAboutCrashAsync("Application Crashed", crash);
            else
                await ReviewReminderAsync();

            #region On update

            if (!AppVersionHelper.JustUpdated) return;

            CurtainPrompt.Show(2500, "AppUpdated".FromLanguageResource(), AppVersionHelper.CurrentVersion);

            //download missing artwork for artist
            if (Locator.CollectionService.IsLibraryLoaded)
                CollectionHelper.DownloadArtistsArtworkAsync();
            else
                Locator.CollectionService.LibraryLoaded +=
                    async (o, args) => CollectionHelper.DownloadArtistsArtworkAsync();

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

        private async void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            //just in case it crashes, save it
            AppSettingsHelper.WriteAsJson("CrashingException", e.Exception);

            await WarnAboutCrashAsync("Crash prevented", e.Exception);
        }

        private async Task WarnAboutCrashAsync(string title, Exception e)
        {
            var stacktrace = e.StackTrace;

            if (stacktrace.Contains("TaskAwaiter"))
            {
                try
                {
                    stacktrace = e.StackTraceEx();
                }
                catch
                {
                }
            }

            const string emailTo = "help@audiotica.fm";
            const string emailSubject = "Audiotica crash report";
            var emailBody = "I encountered a problem with Audiotica...\r\n\r\n" + e.Message + "\r\n\r\nDetails:\r\n" + stacktrace;
            var url = "mailto:?to=" + emailTo + "&subject=" + emailSubject + "&body=" + Uri.EscapeDataString(emailBody);

            if (await MessageBox.ShowAsync(
                   "There was a problem with the application. Do you want to send a crash report so the developer can fix it?",
                   title, MessageBoxButton.OkCancel) == MessageBoxResult.Ok)
            {
                await Launcher.LaunchUriAsync(new Uri(url));
            }
            
            //made it so far, no need to save the crash details
            AppSettingsHelper.Write("CrashingException", null);
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

        private void CreateRootFrame()
        {
            RootFrame = Window.Current.Content as Frame;

            if (RootFrame != null) return;

            RootFrame = new Frame();
            Window.Current.Content = RootFrame;
            DispatcherHelper.Initialize();
        }

        public async Task BootAppServicesAsync()
        {
            if (!_init)
            {
                try
                {
                    await Locator.SqlService.InitializeAsync().Log().ConfigureAwait(false);
                    await Locator.BgSqlService.InitializeAsync().Log().ConfigureAwait(false);

                    Locator.CollectionService.LibraryLoaded += (sender, args) => 
                        DispatcherHelper.RunAsync(() => Locator.Download.LoadDownloads());

                    await Locator.CollectionService.LoadLibraryAsync().Log().ConfigureAwait(false);                     
                }
                catch (Exception ex)
                {
                    EasyTracker.GetTracker().SendException(ex.Message + " " + ex.StackTraceEx(), true);
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