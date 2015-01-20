#region

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Store;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Service.RunTime;
using Audiotica.View;
using Audiotica.ViewModel;
using GalaSoft.MvvmLight.Threading;
using GoogleAnalytics;
using MyToolkit.Paging.Handlers;
using Xamarin;

#endregion

namespace Audiotica
{
    public sealed partial class App
    {
        public static bool IsDebugging = Debugger.IsAttached;
        public static bool IsProduction = false;

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

        public static event EventHandler<BackPressedEventArgs> SupressBackEvent;

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs e)
        {
            if (UiBlockerUtility.SupressBackEvents)
            {
                e.Handled = true;
                if (SupressBackEvent != null)
                    SupressBackEvent(this, e);
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

            var restore = AppSettingsHelper.Read<bool>("FactoryReset")
            || await StorageHelper.FileExistsAsync("_current_restore.autcp");

            if (RootFrame.Content == null)
            {
                Insights.Initialize("38cc9488b4e09fd2c316617d702838ca43a473d4");
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
                        Navigator.GoTo<CollectionArtistPage, ZoomInTransition>(id);
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

            if (AppVersionHelper.JustUpdated)
                OnUpdate();
            else if (AppVersionHelper.IsFirstRun)
                AppSettingsHelper.WriteAsJson("LastRunVersion", AppVersionHelper.CurrentVersion);
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
            if (crash != null && !AppVersionHelper.JustUpdated)
                await WarnAboutCrashAsync("Application Crashed", crash);
            else
                await ReviewReminderAsync();
        }

        private async void OnUpdate()
        {
            //download missing artwork for artist
            await CollectionHelper.DownloadArtistsArtworkAsync();
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
            Insights.Report(e.Exception);
            UiBlockerUtility.Unblock();
            e.Handled = true;
            //just in case it crashes, save it
            AppSettingsHelper.WriteAsJson("CrashingException", e.Exception);

            await WarnAboutCrashAsync("Crash prevented", e.Exception);
        }

        private async Task WarnAboutCrashAsync(string title, Exception e)
        {
            var stacktrace = e.StackTrace;

            const string emailTo = "badbug@audiotica.fm";
            const string emailSubject = "Audiotica crash report";
            var emailBody = "I encountered a problem with Audiotica...\r\n\r\n" + e.Message + "\r\n\r\nDetails:\r\n" +
                            stacktrace;
            var url = "mailto:?to=" + emailTo + "&subject=" + emailSubject + "&body=" + WebUtility.UrlEncode(emailBody);

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

            RootFrame = new Frame() {Style = Resources["AppFrame"] as Style};
            Window.Current.Content = RootFrame;
            DispatcherHelper.Initialize();
        }

        public async Task BootAppServicesAsync()
        {
            if (!_init)
            {
                using (var handle = Insights.TrackTime("Boot App Services"))
                {
                    try
                    {
                        await Locator.SqlService.InitializeAsync().ConfigureAwait(false);
                        await Locator.BgSqlService.InitializeAsync().ConfigureAwait(false);
                        await Locator.CollectionService.LoadLibraryAsync().ConfigureAwait(false);
                        handle.Data.Add("SongCount", Locator.CollectionService.Songs.Count.ToString());

                        DispatcherHelper.RunAsync(() => Locator.Download.LoadDownloads());
                    }
                    catch (Exception ex)
                    {
                        Insights.Report(ex, "Where", "Booting App Services");
                        DispatcherHelper.RunAsync(
                            () => CurtainPrompt.ShowError("AppErrorBooting".FromLanguageResource()));
                    }

                    _init = true;
                }
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
                Insights.Track("Review Reminder", "Accepted", "True");
                Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
            }
            else
            {
                Insights.Track("Review Reminder", "Accepted", "False");
            }
        }
    }
}