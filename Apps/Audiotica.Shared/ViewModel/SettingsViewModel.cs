#region

using System;
using Windows.ApplicationModel.Store;
using Windows.UI.Xaml;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Service.Interfaces;
using Audiotica.PartialView;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GoogleAnalytics;
using GoogleAnalytics.Core;

#endregion

namespace Audiotica.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IScrobblerService _service;
        private string _password;
        private bool _scrobbleSwitch;
        private string _username;

        public SettingsViewModel(IScrobblerService service)
        {
            _service = service;
            InAppAdsClickRelay = new RelayCommand(InAppAdsClicked);
            LoginClickRelay = new RelayCommand(LoginButtonClicked);
            DeveloperModeClickRelay = new RelayCommand(DeveloperModeExecute);

            var creds = CredentialHelper.GetCredentials("lastfm");
            if (creds == null) return;

            LastFmUsername = creds.UserName;
            creds.RetrievePassword();
            LastFmPassword = creds.Password;
            IsLoggedIn = true;
        }

        private async void InAppAdsClicked()
        {
            try
            {
                if (App.IsDebugging)
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
                CurtainPrompt.Show("You can now go the app pivot to disable ads.");

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
            get { return AppSettingsHelper.Read<bool>("DevMode"); }
            set
            {
                AppSettingsHelper.Write("DevMode", value);
                EasyTracker.GetTracker().SendEvent("Settings", "DevMode", value ? "Enabled" : "Disabled", 0);
                RaisePropertyChanged();
            }
        }

        public bool SimulateFirstRun
        {
            get { return AppSettingsHelper.Read<bool>("SimulateFirstRun"); }
            set
            {
                AppSettingsHelper.Write("SimulateFirstRun", value);
                EasyTracker.GetTracker().SendEvent("Settings", "SimulateFirstRun", value ? "Enabled" : "Disabled", 0);
                RaisePropertyChanged();
            }
        }

        public bool SimulateUpdate
        {
            get { return AppSettingsHelper.Read<bool>("SimulateUpdate"); }
            set
            {
                AppSettingsHelper.Write("SimulateUpdate", value);
                EasyTracker.GetTracker().SendEvent("Settings", "SimulateUpdate", value ? "Enabled" : "Disabled", 0);
                RaisePropertyChanged();
            }
        }

        public bool FrameRateCounter
        {
            get { return AppSettingsHelper.Read<bool>("FrameRateCounter"); }
            set
            {
                Application.Current.DebugSettings.EnableFrameRateCounter = value;
                AppSettingsHelper.Write("FrameRateCounter", value);
                EasyTracker.GetTracker().SendEvent("Settings", "FrameRateCounter", value ? "Enabled" : "Disabled", 0);
                RaisePropertyChanged();
            }
        }

        public bool RedrawRegions
        {
            get { return AppSettingsHelper.Read<bool>("RedrawRegions"); }
            set
            {
                Application.Current.DebugSettings.EnableRedrawRegions = value;
                AppSettingsHelper.Write("RedrawRegions", value);
                EasyTracker.GetTracker().SendEvent("Settings", "RedrawRegions", value ? "Enabled" : "Disabled", 0);
                RaisePropertyChanged();
            }
        }

        #endregion

        public bool IsAdsEnabled
        {
            get { return App.LicenseInformation.ProductLicenses[ProductConstants.InAppAdvertisements].IsActive; }
        }

        public string Version
        {
            get { return AppVersionHelper.CurrentVersion.ToString(); }
        }

        public bool WallpaperArt
        {
            get { return AppSettingsHelper.Read("WallpaperArt", true, SettingsStrategy.Roaming); }
            set
            {
                if (!value)
                    App.Locator.Collection.RandomizeAlbumList.Clear();

                EasyTracker.GetTracker().SendEvent("Settings", "WallpaperArt", value ? "Enabled" : "Disabled", 0);
                AppSettingsHelper.Write("WallpaperArt", value, SettingsStrategy.Roaming);
                RaisePropertyChanged();
            }
        }

        public bool AddToInsert
        {
            get { return AppSettingsHelper.Read("AddToInsert", true, SettingsStrategy.Roaming); }
            set
            {
                EasyTracker.GetTracker().SendEvent("Settings", "AddToInsert", value ? "Enabled" : "Disabled", 0);
                AppSettingsHelper.Write("AddToInsert", value, SettingsStrategy.Roaming);
                RaisePropertyChanged();
            }
        }

        public bool Advertisements
        {
            get { return AppSettingsHelper.Read("Ads", true, SettingsStrategy.Roaming); }
            set
            {
                if (value)
                {
                    App.Locator.Ads.Enable();
                }
                else
                {
                    App.Locator.Ads.Disable();
                }
                EasyTracker.GetTracker().SendEvent("Settings", "Ads", value ? "Enabled" : "Disabled", 0);
                AppSettingsHelper.Write("Ads", value, SettingsStrategy.Roaming);
                RaisePropertyChanged();
            }
        }

        public bool Scrobble
        {
            get { return AppSettingsHelper.Read<bool>("Scrobble", SettingsStrategy.Roaming); }
            set
            {
                AppSettingsHelper.Write("Scrobble", value, SettingsStrategy.Roaming);
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
                    }
                    else
                    {
                        CurtainPrompt.ShowError("AuthLoginError".FromLanguageResource());
                    }
                    UiBlockerUtility.Unblock();
                }
            }
        }
    }
}