#region

using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Service.Interfaces;
using Audiotica.PartialView;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using IF.Lastfm.Core.Api;

#endregion

namespace Audiotica.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IScrobblerService _service;
        private readonly AdMediatorBar _adMediatorBar;
        private string _password;
        private string _username;
        private RelayCommand _loginButtonRelay;
        private bool _scrobbleSwitch;

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
            IsLogin = true;
        }

        public string Version
        {
            get { return BetaChangelogHelper.CurrentVersion.ToString(); }
        }

        private async void LoginButtonClicked()
        {
            if (_service.IsAuthenticated)
            {
                _service.Logout();
                CurtainToast.Show("Logged out");
                LastFmUsername = null;
                LastFmPassword = null;
                IsLogin = false;
                Scrobble = false;
            }
            else
            {
                if (await _service.AuthenticaAsync(LastFmUsername, LastFmPassword))
                {
                    CurtainToast.Show("Login successful");
                    IsLogin = true;
                }
                else
                {
                    CurtainToast.ShowError("Failed to login");
                }
            }
        }

        public bool WallpaperArt
        {
            get
            {
                return AppSettingsHelper.Read("WallpaperArt", true);
            }
            set
            {
                if (!value)
                    App.Locator.Collection.RandomizeAlbumList.Clear();

                AppSettingsHelper.Write("WallpaperArt", value);
                RaisePropertyChanged();
            }
        }

        public bool Advertisements
        {
            get
            {
                return AppSettingsHelper.Read("Ads", true);
            }
            set
            {
                string label;
                if (value)
                {
                    _adMediatorBar.Enable();
                    label = "enabled";
                }
                else
                {
                    _adMediatorBar.Disable();
                    label = "disabled";
                }
                GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Settings", "Ads", label, 0);
                AppSettingsHelper.Write("Ads", value);
                RaisePropertyChanged();
            }
        }

        public bool Scrobble
        {
            get
            {
                return AppSettingsHelper.Read<bool>("Scrobble");
            }
            set
            {
                AppSettingsHelper.Write("Scrobble", value);
                RaisePropertyChanged();
            }
        }

        public bool IsLogin
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
    }
}