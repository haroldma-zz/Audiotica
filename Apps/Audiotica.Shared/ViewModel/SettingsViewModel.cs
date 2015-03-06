#region

using System;
using System.Diagnostics;

using Windows.ApplicationModel.Store;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Audiotica.Core;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Core.WinRt;
using Audiotica.Core.WinRt.Common;
using Audiotica.Core.WinRt.Utilities;
using Audiotica.Data.Model.AudioticaCloud;
using Audiotica.Data.Service.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GoogleAnalytics;
using GoogleAnalytics.Core;
using Xamarin;

#endregion

namespace Audiotica.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IScrobblerService _service;
        private readonly IAppSettingsHelper _appSettingsHelper;
        private string _password;
        private bool _scrobbleSwitch;
        private string _username;

        public SettingsViewModel(IScrobblerService service, ICredentialHelper credentialHelper, IAppSettingsHelper appSettingsHelper)
        {
            _service = service;
            _appSettingsHelper = appSettingsHelper;
            InAppAdsClickRelay = new RelayCommand(InAppAdsClicked);
            LoginClickRelay = new RelayCommand(LoginButtonClicked);
            DeveloperModeClickRelay = new RelayCommand(DeveloperModeExecute);

            var creds = credentialHelper.GetCredentials("lastfm");
            if (creds == null) return;

            LastFmUsername = creds.GetUsername();
            LastFmPassword = creds.GetPassword();
            IsLoggedIn = true;
        }

        private async void InAppAdsClicked()
        {
            try
            {
                if (Debugger.IsAttached)
                {
                    await CurrentAppSimulator.RequestProductPurchaseAsync(ProductConstants.InAppAdvertisements);
                }
                else
                {
                    await CurrentApp.RequestProductPurchaseAsync(ProductConstants.InAppAdvertisements);
                }

                if (!IsAdsEnabled) return;

                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged("IsAdsEnabled");
                CurtainPrompt.Show("You can now disabled advertisements!");

                var transaction = new Transaction(ProductConstants.InAppAdvertisements, (long) (1.99*1000000));
                EasyTracker.GetTracker().SendTransaction(transaction);
            }
            catch
            {
            }
        }

        public RelayCommand InAppAdsClickRelay { get; set; }

        private int _devCount;
        private const int DevModeCount = 7;

        private void DeveloperModeExecute()
        {
            if (DevMode)
                return;

            _devCount++;

            if (_devCount >= DevModeCount)
            {
                CurtainPrompt.Show("Challenge Completed: Dev Mode Unlock ");
                DevMode = true;
            }

            else if (_devCount > 3)
            {
                CurtainPrompt.Show("{0} click(s) more to...???", DevModeCount - _devCount);
            }
        }

        #region Dev mode

        public bool DevMode
        {
            get { return _appSettingsHelper.Read<bool>("DevMode"); }
            set
            {
                Insights.Track("Toggled Developer Mode", "Enabled", value.ToString());
                _appSettingsHelper.Write("DevMode", value);
                EasyTracker.GetTracker().SendEvent("Settings", "DevMode", value ? "Enabled" : "Disabled", 0);
                RaisePropertyChanged();
            }
        }

        public bool SimulateFirstRun
        {
            get { return _appSettingsHelper.Read<bool>("SimulateFirstRun"); }
            set
            {
                Insights.Track("Toggled Simulate First Run", "Enabled", value.ToString());
                _appSettingsHelper.Write("SimulateFirstRun", value);
                EasyTracker.GetTracker().SendEvent("Settings", "SimulateFirstRun", value ? "Enabled" : "Disabled", 0);
                RaisePropertyChanged();
            }
        }

        public bool SimulateUpdate
        {
            get { return _appSettingsHelper.Read<bool>("SimulateUpdate"); }
            set
            {
                Insights.Track("Toggled Simulate Update", "Enabled", value.ToString());
                _appSettingsHelper.Write("SimulateUpdate", value);
                EasyTracker.GetTracker().SendEvent("Settings", "SimulateUpdate", value ? "Enabled" : "Disabled", 0);
                RaisePropertyChanged();
            }
        }

        public bool FrameRateCounter
        {
            get { return _appSettingsHelper.Read<bool>("FrameRateCounter"); }
            set
            {
                Insights.Track("Toggled Frame Rate Counter", "Enabled", value.ToString());
                Application.Current.DebugSettings.EnableFrameRateCounter = value;
                _appSettingsHelper.Write("FrameRateCounter", value);
                EasyTracker.GetTracker().SendEvent("Settings", "FrameRateCounter", value ? "Enabled" : "Disabled", 0);
                RaisePropertyChanged();
            }
        }

        public bool RedrawRegions
        {
            get { return _appSettingsHelper.Read<bool>("RedrawRegions"); }
            set
            {
                Insights.Track("Toggled Redraw Regions", "Enabled", value.ToString());
                Application.Current.DebugSettings.EnableRedrawRegions = value;
                _appSettingsHelper.Write("RedrawRegions", value);
                EasyTracker.GetTracker().SendEvent("Settings", "RedrawRegions", value ? "Enabled" : "Disabled", 0);
                RaisePropertyChanged();
            }
        }

        #endregion

        public bool IsAdsEnabled
        {
            get
            {
                var user = App.Locator.AudioticaService.CurrentUser;
                var hasSubscription = user != null && user.Subscription != SubscriptionType.None;


                return !App.IsProduction
                    || hasSubscription
                       || App.LicenseInformation.ProductLicenses[ProductConstants.InAppAdvertisements].IsActive;
            }
        }

        public string Version
        {
            get { return App.Locator.AppVersionHelper.CurrentVersion.ToString(); }
        }

        public bool WallpaperArt
        {
            get { return _appSettingsHelper.Read("WallpaperArt", true, SettingsStrategy.Roaming); }
            set
            {
                // TODO disable wallpaper if already set

                EasyTracker.GetTracker().SendEvent("Settings", "WallpaperArt", value ? "Enabled" : "Disabled", 0);
                Insights.Track("Toggled Wallpaper Art", "Enabled", value.ToString());
                _appSettingsHelper.Write("WallpaperArt", value, SettingsStrategy.Roaming);
                RaisePropertyChanged();
            }
        }

        public bool AddToInsert
        {
            get { return _appSettingsHelper.Read("AddToInsert", true, SettingsStrategy.Roaming); }
            set
            {
                EasyTracker.GetTracker().SendEvent("Settings", "AddToInsert", value ? "Enabled" : "Disabled", 0);
                Insights.Track("Toggled Add To Insert", "Enabled", value.ToString());
                _appSettingsHelper.Write("AddToInsert", value, SettingsStrategy.Roaming);
                RaisePropertyChanged();
            }
        }

        public int SelectedTimeoutOption
        {
            get { return (int)ScreenTimeoutHelper.TimeoutOption; }
            set
            {
                Insights.Track("Changed Timeout", "Option", ((PreventScreenTimeoutOption)value).ToString());
                ScreenTimeoutHelper.TimeoutOption = (PreventScreenTimeoutOption)value;

                if (ScreenTimeoutHelper.TimeoutOption == PreventScreenTimeoutOption.Always)
                    ScreenTimeoutHelper.PreventTimeout();
                else
                    ScreenTimeoutHelper.AllowTimeout();

                EasyTracker.GetTracker().SendEvent("Settings", "SelectedTimeoutOption", ScreenTimeoutHelper.TimeoutOption.ToString(), 0);
                RaisePropertyChanged();
            }
        }

        public bool Advertisements
        {
            get { return _appSettingsHelper.Read("Ads", true, SettingsStrategy.Roaming); }
            set
            {
                Insights.Track("Toggled Advertisements", "Enabled", value.ToString());
                if (value)
                {
                    App.Locator.Ads.Enable();
                }
                else
                {
                    App.Locator.Ads.Disable();
                }
                EasyTracker.GetTracker().SendEvent("Settings", "Ads", value ? "Enabled" : "Disabled", 0);
                _appSettingsHelper.Write("Ads", value, SettingsStrategy.Roaming);
                RaisePropertyChanged();
            }
        }

        public bool Scrobble
        {
            get { return _appSettingsHelper.Read<bool>("Scrobble", SettingsStrategy.Roaming); }
            set
            {
                Insights.Track("Toggled Scrobbling", "Enabled", value.ToString());
                _appSettingsHelper.Write("Scrobble", value, SettingsStrategy.Roaming);
                EasyTracker.GetTracker().SendEvent("Settings", "Scrobble", value ? "Enabled" : "Disabled", 0);
                RaisePropertyChanged();
            }
        }

        public bool IsLoggedIn
        {
            get { return _scrobbleSwitch; }
            set { Set(ref _scrobbleSwitch, value); }
        }

        public string LastFmUsername
        {
            get { return _username; }
            set { Set(ref _username, value); }
        }

        public string LastFmPassword
        {
            get { return _password; }
            set { Set(ref _password, value); }
        }

        public RelayCommand LoginClickRelay { get; set; }
        public RelayCommand DeveloperModeClickRelay { get; set; }

        private async void LoginButtonClicked()
        {
            if (IsLoggedIn)
            {
                _service.Logout();
                CurtainPrompt.Show("AuthLogoutSuccess".FromLanguageResource());
                LastFmUsername = null;
                LastFmPassword = null;
                IsLoggedIn = false;
                Scrobble = false;
                Insights.Track("Logged out from Last.FM");
            }
            else
            {
                if (string.IsNullOrEmpty(LastFmUsername)
                    || string.IsNullOrEmpty(LastFmPassword))
                {
                    CurtainPrompt.ShowError("AuthLoginErrorForgot".FromLanguageResource());
                }

                else
                {
                    UiBlockerUtility.Block("GenericWait".FromLanguageResource());
                    if (await _service.AuthenticaAsync(LastFmUsername, LastFmPassword))
                    {
                        CurtainPrompt.Show("AuthLoginSuccess".FromLanguageResource());
                        IsLoggedIn = true;
                        Scrobble = true;
                        Insights.Track("Logged in Last.FM");
                    }
                    else
                    {
                        CurtainPrompt.ShowError("AuthLoginError".FromLanguageResource());
                    }
                    UiBlockerUtility.Unblock();
                }

                // update the player also
                var msg = new ValueSet { { PlayerConstants.LastFmLoginChanged, string.Empty } };
                BackgroundMediaPlayer.SendMessageToBackground(msg);
            }
        }
    }
}