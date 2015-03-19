using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Audiotica.Core.Utils;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Data.Model;
using Audiotica.Data.Model.AudioticaCloud;
using Audiotica.Data.Service.Interfaces;
using GalaSoft.MvvmLight.Ioc;

namespace Audiotica.Data.Service.RunTime
{
    public class AudioticaService : INotifyPropertyChanged, IAudioticaService
    {
#if DEBUG_WEB
        private const string BaseApiPath = "http://localhost:48065/api/";
        private const string AppToken = "LOCALTESTING";
#else
        private const string BaseApiPath = "https://audiotica-cloud.azure-mobile.net/api/";

        private const string AppToken = "AypzKLKRIDPGkXXzCGYGqjJNliXTwp74";
#endif

        private const string RadioBasePath = BaseApiPath + "radio";
        private const string RadioCreatePath = RadioBasePath + "?artistName={0}";
        private const string RadioLookaheadPath = RadioBasePath + "/{0}/lookahead";
        private const string RadioEventPath = RadioBasePath + "/{0}/event?action={1}&trackId={2}";

        private const string UsersPath = BaseApiPath + "users";

        private const string SubscribePath = UsersPath + "/me/subscribe?planId={0}&coupon={1}";

        private const string TokenPath = UsersPath + "/token";

        private const string MatchPath = BaseApiPath + "match?title={0}&artist={1}&limit={2}";

        private const string SpotlightPath = BaseApiPath + "spotlight?version={0}&os={1}";

        private readonly ICredentialHelper _credentialHelper;

        private readonly IAppSettingsHelper _appSettingsHelper;

        private readonly IDispatcherHelper _dispatcherHelper;

        private readonly INotificationManager _notificationManager;

        public string AuthenticationToken { get; private set; }

        private string _refreshToken;

        private AudioticaUser _currentUser;

        public AudioticaService(
            ICredentialHelper credentialHelper,
            IAppSettingsHelper appSettingsHelper) : this(credentialHelper, appSettingsHelper, null, null)
        {
        }

        [PreferredConstructor]
        public AudioticaService(
            ICredentialHelper credentialHelper,
            IAppSettingsHelper appSettingsHelper,
            IDispatcherHelper dispatcherHelper,
            INotificationManager notificationManager)
        {
            _credentialHelper = credentialHelper;
            _appSettingsHelper = appSettingsHelper;
            _dispatcherHelper = dispatcherHelper;
            _notificationManager = notificationManager;

            var cred = _credentialHelper.GetCredentials("AudioticaCloud");
            if (cred == null)
            {
                return;
            }

            AuthenticationToken = cred.GetPassword();

            CurrentUser = _appSettingsHelper.ReadJsonAs<AudioticaUser>("AudioticaCloudUser");

            var refreshCred = _credentialHelper.GetCredentials("AudioticaCloudRefreshToken");

            if (refreshCred == null)
            {
                return;
            }

            _refreshToken = refreshCred.GetPassword();
        }

        public AudioticaUser CurrentUser
        {
            get { return _currentUser; }

            private set
            {
                _currentUser = value;
                OnPropertyChanged();
                _appSettingsHelper.WriteAsJson("AudioticaCloudUser", value);
            }
        }

        public bool IsAuthenticated
        {
            get { return AuthenticationToken != null; }
        }

        public HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();

            var token = ":" + AppToken;
            token = AuthenticationToken ?? Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            if (AuthenticationToken != null)
            {
                httpClient.DefaultRequestHeaders.Add("X-ZUMO-AUTH", token);
            }
            else
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
            }

            return httpClient;
        }

        #region Helper methods

        private async Task<BaseAudioticaResponse<T>> GetAsync<T>(string url)
        {
            using (var client = CreateHttpClient())
            {
                try
                {
                    using (var resp = await client.GetAsync(url).ConfigureAwait(false))
                    {
                        var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var httpData = await json.DeserializeAsync<BaseAudioticaResponse<T>>().ConfigureAwait(false)
                                       ?? new BaseAudioticaResponse<T>();

                        httpData.StatusCode = resp.StatusCode;
                        httpData.Success = resp.IsSuccessStatusCode;

                        if (resp.StatusCode != HttpStatusCode.Unauthorized)
                        {
                            return httpData;
                        }

                        if (string.IsNullOrEmpty(_refreshToken))
                        {
                            return httpData;
                        }

                        // token expired, refresh it
                        var success = await RefreshTokenAsync();
                        if (success)
                        {
                            // manage to refresh it, repeat the request
                            return await GetAsync<T>(url);
                        }

                        // failed to refresh the token, return the original error response
                        return httpData;
                    }
                }
                catch
                {
                    return new BaseAudioticaResponse<T> {Success = false};
                }
            }
        }

        private async Task<bool> RefreshTokenAsync()
        {
            var data = new Dictionary<string, string> {{"RefreshToken", _refreshToken}};

            Logout();

            var resp = await LoginAsync(data);

            if (!resp.Success && resp.Message != null && resp.Message.Contains("expired") && _dispatcherHelper != null)
            {
                await
                    _dispatcherHelper.RunAsync(
                        () => _notificationManager.ShowError("Please relogin to the Audiotica Cloud."));
            }

            return resp.Success;
        }

        private async Task<BaseAudioticaResponse> PostAsync(string url, Dictionary<string, string> data)
        {
            return await PostAsync<object>(url, data).ConfigureAwait(false) as BaseAudioticaResponse;
        }

        private async Task<BaseAudioticaResponse<T>> PostAsync<T>(string url, Dictionary<string, string> data)
        {
            using (var client = CreateHttpClient())
            using (var content = new FormUrlEncodedContent(data))
            {
                using (var resp = await client.PostAsync(url, content).ConfigureAwait(false))
                {
                    var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var httpData = await json.DeserializeAsync<BaseAudioticaResponse<T>>().ConfigureAwait(false)
                                   ?? new BaseAudioticaResponse<T>();

                    httpData.Success = resp.IsSuccessStatusCode;
                    httpData.StatusCode = resp.StatusCode;

                    if (resp.StatusCode != HttpStatusCode.Unauthorized)
                    {
                        return httpData;
                    }

                    if (string.IsNullOrEmpty(_refreshToken))
                    {
                        return httpData;
                    }

                    // token expired, refresh it
                    var success = await RefreshTokenAsync();
                    if (success)
                    {
                        // manage to refresh it, repeat the request
                        return await PostAsync<T>(url, data);
                    }

                    // failed to refresh the token, return the original error response
                    return httpData;
                }
            }
        }

        #endregion

        public async Task<BaseAudioticaResponse> LoginAsync(string username, string password)
        {
            var data = new Dictionary<string, string> {{"username", username}, {"password", password}};

            return await LoginAsync(data);
        }

        private async Task<BaseAudioticaResponse> LoginAsync(Dictionary<string, string> data)
        {
            var resp = await PostAsync<LoginData>(TokenPath, data);

            if (!resp.Success)
            {
                return resp;
            }

            await SaveLoginStateAsync(resp);

            return resp;
        }

        public async Task<AudioticaSpotlight> GetSpotlightAsync()
        {
            var resp = await GetAsync<AudioticaSpotlight>(string.Format(SpotlightPath, "1", "wp81"));
            return resp.Data;
        }

        private async Task SaveLoginStateAsync(BaseAudioticaResponse<LoginData> resp)
        {
            AuthenticationToken = resp.Data.AuthenticationToken;

            if (_dispatcherHelper != null)
                await _dispatcherHelper.RunAsync(() => CurrentUser = resp.Data.User);
            else
                CurrentUser = resp.Data.User;

            if (!string.IsNullOrEmpty(resp.Data.RefreshToken))
            {
                _refreshToken = resp.Data.RefreshToken;
                _credentialHelper.SaveCredentials("AudioticaCloudRefreshToken", resp.Data.User.Id, _refreshToken);
            }

            _credentialHelper.SaveCredentials("AudioticaCloud", resp.Data.User.Id, resp.Data.AuthenticationToken);

            if (_dispatcherHelper != null)
                await _dispatcherHelper.RunAsync(() => OnPropertyChanged("IsAuthenticated"));
        }

        public async Task<BaseAudioticaResponse> RegisterAsync(string username, string password, string email)
        {
            var data = new Dictionary<string, string>
            {
                {"username", username},
                {"email", email},
                {"password", password}
            };

            var resp = await PostAsync<LoginData>(UsersPath, data);

            if (!resp.Success)
            {
                return resp;
            }

            await SaveLoginStateAsync(resp);

            return resp;
        }

        public async Task<BaseAudioticaResponse<RadioData>> CreateStationAsync(string artistName)
        {
            var url = string.Format(RadioCreatePath, artistName);
            return await GetAsync<RadioData>(url);
        }

        public async Task<BaseAudioticaResponse<RadioData>> StationLookahead(string id)
        {
            var url = string.Format(RadioLookaheadPath, id);
            return await GetAsync<RadioData>(url);
        }

        public async Task<BaseAudioticaResponse<RadioData>> StationEvent(string id, RadioEvent action, string trackId)
        {
            var url = string.Format(RadioEventPath, id, action.ToString().ToLower(), trackId);
            return await GetAsync<RadioData>(url);
        }

        public void Logout()
        {
            AuthenticationToken = null;
            _refreshToken = null;

            _appSettingsHelper.Write("AudioticaCloudRefreshToken", null);
            _appSettingsHelper.Write("AudioticaCloudUser", null);
            _credentialHelper.DeleteCredentials("AudioticaCloud");

            if (_dispatcherHelper != null)
            {
                _dispatcherHelper.RunAsync(() => CurrentUser = null);
                _dispatcherHelper.RunAsync(() => OnPropertyChanged("IsAuthenticated"));
            }
            else
                CurrentUser = null;
        }

        public async Task<BaseAudioticaResponse<AudioticaUser>> GetProfileAsync()
        {
            var resp = await GetAsync<AudioticaUser>(UsersPath + "/me");

            if (!resp.Success)
            {
                return resp;
            }

            // keping the user object updated
            CurrentUser = resp.Data;

            return resp;
        }

        public async Task<BaseAudioticaResponse> SubscribeAsync(
            SubscriptionType plan,
            SubcriptionTimeFrame timeFrame,
            AudioticaStripeCard card,
            string coupon = null)
        {
            var creditCardData = new Dictionary<string, string>
            {
                {"name", card.Name},
                {"number", card.Number},
                {"expMonth", card.ExpMonth.ToString()},
                {"expYear", card.ExpYear.ToString()},
                {"cvc", card.Cvc}
            };

            // plan id and coupon are passed in url query
            var planId = plan == SubscriptionType.Silver ? "autc_silver" : "autc_gold";
            planId += "_" + timeFrame.ToString().ToLower();
            var url = string.Format(SubscribePath, planId, coupon);

            var resp = await PostAsync<LoginData>(url, creditCardData);

            if (resp.Success)
            {
                await SaveLoginStateAsync(resp);
            }

            return resp;
        }

        public async Task<BaseAudioticaResponse<List<WebSong>>> GetMatchesAsync(
            string title,
            string artist,
            int limit = 1)
        {
            var resp =
                await
                    GetAsync<List<WebSong>>(
                        string.Format(MatchPath, Uri.EscapeDataString(title), Uri.EscapeDataString(artist), limit));
            return resp;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}