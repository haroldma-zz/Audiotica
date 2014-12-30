#region

using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Service.Interfaces;
using Audiotica.PartialView;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GoogleAnalytics;

#endregion

namespace Audiotica.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly AdMediatorBar _adMediatorBar;
        private readonly IScrobblerService _service;
        private RelayCommand _loginButtonRelay;
        private string _password;
        private bool _scrobbleSwitch;
        private string _username;

        public SettingsViewModel(IScrobblerService service, AdMediatorBar adMediatorBar)
        {
            _service = service;
            _adMediatorBar = adMediatorBar;
            _loginButtonRelay = new RelayCommand(LoginButtonClicked);

            var creds = CredentialHelper.GetCredentials("lastfm");
            if (creds == null) return;

            LastFmUsername = creds.UserName;
            creds.RetrievePassword();
            LastFmPassword = creds.Password;
            IsLoggedIn = true;
        }

        public string Version
        {
            get { return BetaChangelogHelper.CurrentVersion.ToString(); }
        }

        public bool WallpaperArt
        {
            get { return AppSettingsHelper.Read("WallpaperArt", true); }
            set
            {
                if (!value)
                    App.Locator.Collection.RandomizeAlbumList.Clear();

                EasyTracker.GetTracker().SendEvent("Settings", "WallpaperArt", value ? "Enabled" : "Disabled", 0);
                AppSettingsHelper.Write("WallpaperArt", value);
                RaisePropertyChanged();
            }
        }

        public bool Advertisements
        {
            get { return AppSettingsHelper.Read("Ads", true); }
            set
            {
                if (value)
                {
                    _adMediatorBar.Enable();
                }
                else
                {
                    _adMediatorBar.Disable();
                }
                EasyTracker.GetTracker().SendEvent("Settings", "Ads", value ? "Enabled" : "Disabled", 0);
                AppSettingsHelper.Write("Ads", value);
                RaisePropertyChanged();
            }
        }

        public bool Scrobble
        {
            get { return AppSettingsHelper.Read<bool>("Scrobble"); }
            set
            {
                AppSettingsHelper.Write("Scrobble", value);
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

        public RelayCommand LoginButtonRelay
        {
            get { return _loginButtonRelay; }
            set { Set(ref _loginButtonRelay, value); }
        }

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
                    CurtainPrompt.Show("GenericWait".FromLanguageResource());
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
                }
            }
        }
    }
}