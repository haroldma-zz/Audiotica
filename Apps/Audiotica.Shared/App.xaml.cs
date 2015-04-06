using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Store;
using Windows.Graphics.Display;
using Windows.Media.SpeechRecognition;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Utilities;
using Audiotica.Core.WinRt;
using Audiotica.Core.WinRt.Common;
using Audiotica.Data.Collection.Model;
using Audiotica.View;
using Audiotica.ViewModel;
using GalaSoft.MvvmLight.Threading;
using GoogleAnalytics;
using SQLite;
using Xamarin;

namespace Audiotica
{
    /// <summary>
    ///     Class App. This class cannot be inherited.
    /// </summary>
    public sealed partial class App
    {
        public const bool IsProduction = true;

        #region Constructor

        public App()
        {
            InitializeComponent();
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
            _continuationManager = new ContinuationManager();
            Suspending += OnSuspending;
            Resuming += OnResuming;
            UnhandledException += OnUnhandledException;
        }

        #endregion

        public static event EventHandler<BackPressedEventArgs> SupressBackEvent;

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
            get { return Debugger.IsAttached ? CurrentAppSimulator.LicenseInformation : CurrentApp.LicenseInformation; }
        }

        #endregion

        #region Overriding

        protected override void OnActivated(IActivatedEventArgs e)
        {
            base.OnActivated(e);

            StartApp();

            if (e.Kind == ActivationKind.VoiceCommand)
            {
                var commandArgs = (VoiceCommandActivatedEventArgs) e;
                var speechRecognitionResult = commandArgs.Result;

                // If so, get the name of the voice command, the actual text spoken, and the value of Command/Navigate@Target.
                var voiceCommandName = speechRecognitionResult.RulePath[0];

                switch (voiceCommandName)
                {
                    case "Search":
                        var term = speechRecognitionResult.SemanticInterpretation.Properties["term"].FirstOrDefault();
                        Navigator.GoTo<SearchPage, ZoomInTransition>(term);
                        break;
                    case "PlayEntry":
                        var entryName =
                            speechRecognitionResult.SemanticInterpretation.Properties["entry"].FirstOrDefault()
                                .ToLower();
                        CollectionHelper.RequiresCollectionToLoad(
                            async () =>
                            {
                                var artist =
                                    Locator.CollectionService.Artists.FirstOrDefault(
                                        p => p.Name.ToLower().Contains(entryName));

                                List<Song> songs = null;

                                if (artist != null)
                                {
                                    songs = artist.Songs.OrderBy(p => p.Name).ToList();
                                }
                                else
                                {
                                    var album =
                                        Locator.CollectionService.Albums.FirstOrDefault(
                                            p => p.Name.ToLower().Contains(entryName));
                                    if (album != null)
                                    {
                                        songs = album.Songs.OrderBy(p => p.Name).ToList();
                                    }
                                }

                                if (songs != null)
                                {
                                    await CollectionHelper.PlaySongsAsync(songs);
                                }
                                else
                                {
                                    CurtainPrompt.ShowError("Coudln't find that album or artist in your collection.");
                                }
                            });
                        break;
                    case "PlaySong":
                        var songName =
                            speechRecognitionResult.SemanticInterpretation.Properties["song"].FirstOrDefault().ToLower();
                        var artistName =
                            speechRecognitionResult.SemanticInterpretation.Properties["entry"].FirstOrDefault()
                                .ToLower();

                        if (artistName == "any artist")
                        {
                            artistName = string.Empty;
                        }

                        CollectionHelper.RequiresCollectionToLoad(
                            async () =>
                            {
                                var song =
                                    Locator.CollectionService.Songs.FirstOrDefault(
                                        p =>
                                            p.Name.ToLower().Contains(songName)
                                            && (p.Artist.Name.ToLower().Contains(artistName)
                                                || p.ArtistName.ToLower().Contains(artistName)));

                                if (song == null)
                                {
                                    var album =
                                        Locator.CollectionService.Albums.FirstOrDefault(
                                            p =>
                                                p.Name.ToLower().Contains(songName)
                                                && p.PrimaryArtist.Name.ToLower().Contains(artistName));

                                    if (album == null)
                                    {
                                        CurtainPrompt.ShowError("Couldn't find that song or album in your collection.");
                                    }
                                    else
                                    {
                                        await CollectionHelper.PlaySongsAsync(album.Songs.OrderBy(p => p.Name).ToList());
                                    }
                                }
                                else
                                {
                                    var queue =
                                        Locator.CollectionService.CurrentPlaybackQueue.FirstOrDefault(
                                            p => p.SongId == song.Id);

                                    if (queue == null)
                                    {
                                        await
                                            CollectionHelper.PlaySongsAsync(
                                                song,
                                                Locator.CollectionService.Songs.OrderBy(p => p.Name).ToList());
                                    }
                                    else
                                    {
                                        Locator.AudioPlayerHelper.PlaySong(queue);
                                    }
                                }
                            });
                        break;
                    case "NowPlaying":
                        CollectionHelper.RequiresCollectionToLoad(
                            () =>
                            {
                                if (Locator.Player.IsPlayerActive)
                                {
                                    NowPlayingSheetUtility.OpenNowPlaying();
                                }
                                else
                                {
                                    CurtainPrompt.ShowError("Nothing playing right now.");
                                }
                            });
                        break;
                }
            }
            else
            {
                var continuationEventArgs = e as IContinuationActivatedEventArgs;

                if (continuationEventArgs != null)
                {
                    _continuationManager.Continue(continuationEventArgs);
                }
            }
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            StartApp(e.Arguments);
        }

        #endregion

        #region Events

        private async void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            RootFrame.Navigated -= RootFrame_FirstNavigated;

            ScreenTimeoutHelper.OnLaunched();

            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            ApplicationView.GetForCurrentView().VisibleBoundsChanged += OnVisibleBoundsChanged;
            OnVisibleBoundsChanged(null, null);

            var crash = Locator.AppSettingsHelper.ReadJsonAs<Exception>("CrashingException");
            if (crash != null && !Locator.AppVersionHelper.JustUpdated)
            {
                await WarnAboutCrashAsync("Application Crashed", crash);
            }
            else
            {
                await ReviewReminderAsync();
            }

            CollectionHelper.RequiresCollectionToLoad(
                async () =>
                {
                    // downloading missing artwork and match mp3 songs where they haven't been
                    CollectionHelper.MatchSongs();
                    await CollectionHelper.DownloadAlbumsArtworkAsync();
                    await CollectionHelper.DownloadArtistsArtworkAsync();

                    await CollectionHelper.CloudSync();
                },
                false);
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

            scaledImageSize = (int) (scaledImageSize*factor);
            return scaledImageSize;
        }

        private void OnUpdate()
        {
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
            Locator.Player.OnAppActive();
            Locator.SqlService.Initialize();
            Locator.BgSqlService.Initialize();
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            Locator.AudioPlayerHelper.OnAppSuspending();
            Locator.Player.OnAppSuspending();
            Locator.SqlService.Dispose();
            Locator.BgSqlService.Dispose();
            deferral.Complete();
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Insights.Report(e.Exception);
            e.Handled = true;

            if (e.Message.Contains("No installed components were detected."))
            {
                // bug with Mvvmlight, hoping they get it fix soon.  Does not affect the app at all, no need to bug the user with it.
                // EDIT: seems to be another bug that happens with AdMediator also
                return;
            }

            // just in case it crashes, save it
            Locator.AppSettingsHelper.WriteAsJson("CrashingException", e.Exception);

            DispatcherHelper.CheckBeginInvokeOnUI(
                async () =>
                {
                    UiBlockerUtility.Unblock();
                    await WarnAboutCrashAsync("Crash prevented", e.Exception);
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

            var url = "http://www.windowsphone.com/s?appid=" + CurrentApp.AppId;
            request.Data.SetText(request.Data.Properties.Description + " " + url);
        }

        #endregion

        public void StartApp(string argument = "")
        {
            CreateRootFrame();

            var restore = Locator.AppSettingsHelper.Read<bool>("FactoryReset")
                          || Locator.AppSettingsHelper.Read<bool>("Restore");

            if (RootFrame.Content == null)
            {
                Insights.Initialize("38cc9488b4e09fd2c316617d702838ca43a473d4");
                CollectionHelper.IdentifyXamarin();
                RootFrame.Navigated += RootFrame_FirstNavigated;

                // MainPage is always in rootFrame so we don't have to worry about restoring the navigation state on resume
                RootFrame.Navigate(typeof (RootPage), null);
            }

            if (argument.StartsWith("artists/"))
            {
                NowPlayingSheetUtility.CloseNowPlaying();

                if (Navigator.CurrentPage is CollectionArtistPage)
                {
                    Navigator.GoBack();
                }

                var id = int.Parse(argument.Replace("artists/", string.Empty));

                CollectionHelper.RequiresCollectionToLoad(
                    () =>
                    {
                        if (Locator.CollectionService.Artists.FirstOrDefault(p => p.Id == id) != null)
                        {
                            Navigator.GoTo<CollectionArtistPage, ZoomInTransition>(id);
                        }
                    });
            }
            else if (argument.StartsWith("albums/"))
            {
                NowPlayingSheetUtility.CloseNowPlaying();

                if (Navigator.CurrentPage is CollectionAlbumPage)
                {
                    Navigator.GoBack();
                }

                var id = int.Parse(argument.Replace("albums/", string.Empty));
                CollectionHelper.RequiresCollectionToLoad(
                    () =>
                    {
                        if (Locator.CollectionService.Albums.FirstOrDefault(p => p.Id == id) != null)
                        {
                            Navigator.GoTo<CollectionAlbumPage, ZoomInTransition>(id);
                        }
                    });
            }

            // Ensure the current window is active
            Window.Current.Activate();

            // ReSharper disable once CSharpWarnings::CS4014
            if (!restore)
            {
                BootAppServicesAsync();
            }

            var dataManager = DataTransferManager.GetForCurrentView();
            dataManager.DataRequested += DataTransferManagerOnDataRequested;

            if (Locator.AppVersionHelper.JustUpdated)
            {
                OnUpdate();
            }
            else if (Locator.AppVersionHelper.IsFirstRun)
            {
                Locator.AppSettingsHelper.WriteAsJson("LastRunVersion", Locator.AppVersionHelper.CurrentVersion);
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

            RootFrame = new Frame {Style = Resources["AppFrame"] as Style};
            Window.Current.Content = RootFrame;
        }

        public async Task BootAppServicesAsync()
        {
            if (!_init)
            {
                Locator.CollectionService.ScaledImageSize = GetScaledImageSize();

                using (var handle = Insights.TrackTime("Boot App Services"))
                {
                    StatusBar.GetForCurrentView().ForegroundColor = Colors.White;
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

                        var storageFile =
                            await
                                StorageFile.GetFileFromApplicationUriAsync(
                                    new Uri("ms-appx:///AudioticaCommands.xml"));
                        await
                            VoiceCommandManager.InstallCommandSetsFromStorageFileAsync(
                                storageFile);
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
            Locator.Player.OnAppActive();
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