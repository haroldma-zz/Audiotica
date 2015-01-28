using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Audiotica.Core.Utilities;
using Audiotica.Core.Utils;
using Audiotica.Core.WinRt;
using Audiotica.Core.WinRt.Common;
using Audiotica.View;
using Audiotica.ViewModel;

using GalaSoft.MvvmLight.Threading;

using GoogleAnalytics;

using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Store;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using SQLite;

using Xamarin;

namespace Audiotica
{
    /// <summary>
    /// Class App. This class cannot be inherited.
    /// </summary>
    public sealed partial class App
    {
        public const bool IsProduction = true;

        #region Fields

#if WINDOWS_PHONE_APP
        private readonly ContinuationManager continuationManager;
#endif

        private bool _init;

        private static ViewModelLocator locator;

        #endregion

        public static event EventHandler<BackPressedEventArgs> SupressBackEvent;

        #region Properties

        public static Navigator Navigator { get; set; }

        public static ViewModelLocator Locator
        {
            get
            {
                return locator ?? (locator = Current.Resources["Locator"] as ViewModelLocator);
            }
        }

        public static Frame RootFrame { get; private set; }

        public static LicenseInformation LicenseInformation
        {
            get
            {
                return Debugger.IsAttached ? CurrentAppSimulator.LicenseInformation : CurrentApp.LicenseInformation;
            }
        }

        #endregion

        #region Constructor

        public App()
        {
            this.InitializeComponent();
            HardwareButtons.BackPressed += this.HardwareButtonsOnBackPressed;
            this.continuationManager = new ContinuationManager();
            this.Suspending += this.OnSuspending;
            this.Resuming += this.OnResuming;
            this.UnhandledException += this.OnUnhandledException;
        }

        #endregion

        #region Overriding

        protected override void OnActivated(IActivatedEventArgs e)
        {
            base.OnActivated(e);

            this.CreateRootFrame();

            if (RootFrame.Content == null)
            {
                RootFrame.Navigated += this.RootFrame_FirstNavigated;
                RootFrame.Navigate(typeof(RootPage));
            }

            var continuationEventArgs = e as IContinuationActivatedEventArgs;

            if (continuationEventArgs != null)
            {
                this.continuationManager.Continue(continuationEventArgs);
            }

            Window.Current.Activate();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            this.CreateRootFrame();

            var restore = Locator.AppSettingsHelper.Read<bool>("FactoryReset")
                          || await StorageHelper.FileExistsAsync("_current_restore.autcp");

            if (RootFrame.Content == null)
            {
                Insights.Initialize("38cc9488b4e09fd2c316617d702838ca43a473d4");
                RootFrame.Navigated += this.RootFrame_FirstNavigated;

                // MainPage is always in rootFrame so we don't have to worry about restoring the navigation state on resume
                RootFrame.Navigate(typeof(RootPage), e.Arguments);
            }

            if (e.Arguments.StartsWith("artists/"))
            {
                NowPlayingSheetUtility.CloseNowPlaying();

                if (Navigator.CurrentPage is CollectionArtistPage)
                {
                    Navigator.GoBack();
                }

                var id = int.Parse(e.Arguments.Replace("artists/", string.Empty));

                if (Locator.CollectionService.Artists.FirstOrDefault(p => p.Id == id) != null)
                {
                    Navigator.GoTo<CollectionArtistPage, ZoomInTransition>(id);
                }
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
                {
                    Navigator.GoBack();
                }

                var id = int.Parse(e.Arguments.Replace("albums/", string.Empty));

                if (Locator.CollectionService.Albums.FirstOrDefault(p => p.Id == id) != null)
                {
                    Navigator.GoTo<CollectionAlbumPage, ZoomInTransition>(id);
                }
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
            {
                this.BootAppServicesAsync();
            }

            var dataManager = DataTransferManager.GetForCurrentView();
            dataManager.DataRequested += this.DataTransferManagerOnDataRequested;

            if (Locator.AppVersionHelper.JustUpdated)
            {
                this.OnUpdate();
            }
            else if (Locator.AppVersionHelper.IsFirstRun)
            {
                Locator.AppSettingsHelper.WriteAsJson("LastRunVersion", Locator.AppVersionHelper.CurrentVersion);
            }
        }

        #endregion

        #region Events

        private async void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            RootFrame.Navigated -= this.RootFrame_FirstNavigated;

            ScreenTimeoutHelper.OnLaunched();

            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            ApplicationView.GetForCurrentView().VisibleBoundsChanged += this.OnVisibleBoundsChanged;
            this.OnVisibleBoundsChanged(null, null);

            var crash = Locator.AppSettingsHelper.ReadJsonAs<Exception>("CrashingException");
            if (crash != null && !Locator.AppVersionHelper.JustUpdated)
            {
                await this.WarnAboutCrashAsync("Application Crashed", crash);
            }
            else
            {
                await this.ReviewReminderAsync();
            }
        }

        private int GetScaledImageSize()
        {
            var scaledImageSize = 200;
            double factor = 1;

            var scaledFactor = DisplayInformation.GetForCurrentView().ResolutionScale;
            switch (scaledFactor)
            {
                case ResolutionScale.Scale120Percent:
                    factor = 1.2;
                    break;
                case ResolutionScale.Scale140Percent:
                    factor = 1.4;
                    break;
                case ResolutionScale.Scale150Percent:
                    factor = 1.5;
                    break;
                case ResolutionScale.Scale160Percent:
                    factor = 1.6;
                    break;
                case ResolutionScale.Scale180Percent:
                    factor = 1.8;
                    break;
                case ResolutionScale.Scale225Percent:
                    factor = 2.25;
                    break;
            }

            scaledImageSize = (int)(scaledImageSize * factor);
            return scaledImageSize;
        }

        private async void OnUpdate()
        {
            // download missing artwork for artist
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

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Insights.Report(e.Exception);
            e.Handled = true;

            // just in case it crashes, save it
            Locator.AppSettingsHelper.WriteAsJson("CrashingException", e.Exception);

            DispatcherHelper.CheckBeginInvokeOnUI(
                async () =>
                    {
                        UiBlockerUtility.Unblock();
                        await this.WarnAboutCrashAsync("Crash prevented", e.Exception);
                    });
        }

        private async Task WarnAboutCrashAsync(string title, Exception e)
        {
            var stacktrace = e.StackTrace;

            const string emailTo = "badbug@audiotica.fm";
            const string emailSubject = "Audiotica crash report";
            var emailBody = "I encountered a problem with Audiotica (v" + Locator.AppVersionHelper.CurrentVersion
                            + ")...\r\n\r\n" + e.Message + "\r\n\r\nDetails:\r\n" + stacktrace;
            var url = "mailto:?to=" + emailTo + "&subject=" + emailSubject + "&body=" + Uri.EscapeDataString(emailBody);

            if (
                await
                MessageBox.ShowAsync(
                    "There was a problem with the application. Do you want to send a crash report so the developer can fix it?", 
                    title, 
                    MessageBoxButton.OkCancel) == MessageBoxResult.Ok)
            {
                await Launcher.LaunchUriAsync(new Uri(url));
            }

            // made it so far, no need to save the crash details
            Locator.AppSettingsHelper.Write("CrashingException", null);
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

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs e)
        {
            if (UiBlockerUtility.SupressBackEvents)
            {
                e.Handled = true;
                if (SupressBackEvent != null)
                {
                    SupressBackEvent(this, e);
                }
            }
            else if (Navigator.GoBack())
            {
                e.Handled = true;
            }
        }

        private void CreateRootFrame()
        {
            RootFrame = Window.Current.Content as Frame;

            if (RootFrame != null)
            {
                return;
            }

            DispatcherHelper.Initialize();

            Locator.AppVersionHelper.OnLaunched();
            EasyTracker.GetTracker().AppVersion = Locator.AppVersionHelper.CurrentVersion
                                                  + (IsProduction ? string.Empty : "-beta");

            Current.DebugSettings.EnableFrameRateCounter = Locator.AppSettingsHelper.Read<bool>("FrameRateCounter");
            Current.DebugSettings.EnableRedrawRegions = Locator.AppSettingsHelper.Read<bool>("RedrawRegions");

            RootFrame = new Frame { Style = this.Resources["AppFrame"] as Style };
            Window.Current.Content = RootFrame;
        }

        public async Task BootAppServicesAsync()
        {
            if (!this._init)
            {
                Locator.CollectionService.ScaledImageSize = this.GetScaledImageSize();

                using (var handle = Insights.TrackTime("Boot App Services"))
                {
                    try
                    {
                        try
                        {
                            await Locator.SqlService.InitializeAsync().ConfigureAwait(false);
                        }
                        catch (SQLiteException ex)
                        {
                            if (ex.Message.Contains("IOError") || ex.Message.Contains("I/O"))
                            {
                                // issues when SQLite can't delete the wal related files, so init in delete mode
                                // and then go back to wal mode
                                Locator.SqlService.Dispose();
                                Locator.SqlService.Initialize(false);
                                Locator.SqlService.Dispose();
                                Locator.SqlService.Initialize();
                            }
                        }

                        try
                        {
                            await Locator.BgSqlService.InitializeAsync().ConfigureAwait(false);
                        }
                        catch (SQLiteException ex)
                        {
                            if (ex.Message.Contains("IOError") || ex.Message.Contains("I/O"))
                            {
                                // issues when SQLite can't delete the wal related files, so init in delete mode
                                // and then go back to wal mode
                                Locator.BgSqlService.Dispose();
                                Locator.BgSqlService.Initialize(false);
                                Locator.BgSqlService.Dispose();
                                Locator.BgSqlService.Initialize();
                            }
                        }

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

                    this._init = true;
                }
            }

            Locator.AudioPlayerHelper.OnAppActive();
        }

        private async Task ReviewReminderAsync()
        {
            var launchCount = Locator.AppSettingsHelper.Read<int>("LaunchCount");
            Locator.AppSettingsHelper.Write("LaunchCount", ++launchCount);
            if (launchCount != 5)
            {
                return;
            }

            var rate = "FeedbackDialogRateButton".FromLanguageResource();

            var md = new MessageDialog(
                "FeedbackDialogContent".FromLanguageResource(), 
                "FeedbackDialogTitle".FromLanguageResource());
            md.Commands.Add(new UICommand(rate));
            md.Commands.Add(new UICommand("FeedbackDialogNoButton".FromLanguageResource()));

            var result = await md.ShowAsync();

            if (result != null && result.Label == rate)
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