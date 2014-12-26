#region

using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Service.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using IF.Lastfm.Core.Api;

#endregion

namespace Audiotica.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IScrobblerService _service;
        private string _password;
        private string _username;
        private RelayCommand _loginButtonRelay;
        private bool _scrobbleSwitch;

        public SettingsViewModel(IScrobblerService service)
        {
            _service = service;
            _loginButtonRelay = new RelayCommand(LoginButtonClicked);

            var creds = CredentialHelper.GetCredentials("lastfm");
            if (creds == null) return;

            LastFmUsername = creds.UserName;
            creds.RetrievePassword();
            LastFmPassword = creds.Password;
            ScrobbleSwitchEnabled = true;
        }

        private async void LoginButtonClicked()
        {
            if (_service.IsAuthenticated)
            {
                _service.Logout();
                CurtainToast.Show("Logged out");
                LastFmUsername = null;
                LastFmPassword = null;
                ScrobbleSwitchEnabled = false;
                Scrobble = false;
            }
            else
            {
                if (await _service.AuthenticaAsync(LastFmUsername, LastFmPassword))
                {
                    CurtainToast.Show("Login successful");
                    ScrobbleSwitchEnabled = true;
                }
                else
                {
                    CurtainToast.ShowError("Failed to login");
                }
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

        public bool ScrobbleSwitchEnabled
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