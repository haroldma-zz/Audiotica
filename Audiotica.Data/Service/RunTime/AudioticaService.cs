#region

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Audiotica.Core.Utils;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Data.Model;
using Audiotica.Data.Model.AudioticaCloud;
using Audiotica.Data.Service.Interfaces;

using GalaSoft.MvvmLight;

#endregion

namespace Audiotica.Data.Service.RunTime
{
    public class AudioticaService : ObservableObject, IAudioticaService
    {
#if DEBUG_WEB
        private const string BaseApiPath = "http://localhost:48065/api/";
        private const string AppToken = "LOCALTESTING";
#else
        private const string BaseApiPath = "https://audiotica-cloud.azure-mobile.net/api/";

        private const string AppToken = "AypzKLKRIDPGkXXzCGYGqjJNliXTwp74";
#endif

        private const string UsersPath = BaseApiPath + "users";

        private const string SubscribePath = UsersPath + "/me/subscribe?planId={0}&coupon={1}";

        private const string TokenPath = UsersPath + "/token";

        private const string MatchPath = BaseApiPath + "match?title={0}&artist={1}&limit={2}";

        private const string SpotlightPath = BaseApiPath + "spotlight?version={0}&os={1}";

        private readonly ICredentialHelper credentialHelper;

        private readonly IAppSettingsHelper appSettingsHelper;

        private readonly IDispatcherHelper _dispatcherHelper;

        private readonly INotificationManager _notificationManager;

        public string AuthenticationToken { get; private set; }

        private string refreshToken;

        private AudioticaUser currentUser;

        public AudioticaService(
            ICredentialHelper credentialHelper, 
            IAppSettingsHelper appSettingsHelper, 
            IDispatcherHelper dispatcherHelper, 
            INotificationManager notificationManager)
        {
            this.credentialHelper = credentialHelper;
            this.appSettingsHelper = appSettingsHelper;
            _dispatcherHelper = dispatcherHelper;
            _notificationManager = notificationManager;

            var cred = this.credentialHelper.GetCredentials("AudioticaCloud");
            if (cred == null)
            {
                return;
            }

            AuthenticationToken = cred.GetPassword();

            CurrentUser = this.appSettingsHelper.ReadJsonAs<AudioticaUser>("AudioticaCloudUser");

            var refreshCred = this.credentialHelper.GetCredentials("AudioticaCloudRefreshToken");

            if (refreshCred == null)
            {
                return;
            }

            refreshToken = refreshCred.GetPassword();
        }

        public AudioticaUser CurrentUser
        {
            get
            {
                return currentUser;
            }

            private set
            {
                Set(ref currentUser, value);
                appSettingsHelper.WriteAsJson("AudioticaCloudUser", value);
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return AuthenticationToken != null;
            }
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
                var resp = await client.GetAsync(url).ConfigureAwait(false);
                var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                var httpData = await json.DeserializeAsync<BaseAudioticaResponse<T>>().ConfigureAwait(false)
                               ?? new BaseAudioticaResponse<T>();

                httpData.StatusCode = resp.StatusCode;
                httpData.Success = resp.IsSuccessStatusCode;

                if (resp.StatusCode != HttpStatusCode.Unauthorized)
                {
                    return httpData;
                }

                if (string.IsNullOrEmpty(refreshToken))
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

        private async Task<bool> RefreshTokenAsync()
        {
            var data = new Dictionary<string, string> { { "RefreshToken", refreshToken } };

            Logout();

            var resp = await LoginAsync(data);

            if (!resp.Success && resp.Message != null && resp.Message.Contains("expired"))
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
                var resp = await client.PostAsync(url, content).ConfigureAwait(false);
                var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                var httpData = await json.DeserializeAsync<BaseAudioticaResponse<T>>().ConfigureAwait(false)
                               ?? new BaseAudioticaResponse<T>();

                httpData.Success = resp.IsSuccessStatusCode;
                httpData.StatusCode = resp.StatusCode;

                if (resp.StatusCode != HttpStatusCode.Unauthorized)
                {
                    return httpData;
                }

                if (string.IsNullOrEmpty(refreshToken))
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

        #endregion

        public async Task<BaseAudioticaResponse> LoginAsync(string username, string password)
        {
            var data = new Dictionary<string, string> { { "username", username }, { "password", password } };

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
            await _dispatcherHelper.RunAsync(() => CurrentUser = resp.Data.User);

            if (!string.IsNullOrEmpty(resp.Data.RefreshToken))
            {
                refreshToken = resp.Data.RefreshToken;
                credentialHelper.SaveCredentials("AudioticaCloudRefreshToken", resp.Data.User.Id, refreshToken);
            }

            credentialHelper.SaveCredentials("AudioticaCloud", resp.Data.User.Id, resp.Data.AuthenticationToken);
            await _dispatcherHelper.RunAsync(() => RaisePropertyChanged(() => IsAuthenticated));
        }

        public async Task<BaseAudioticaResponse> RegisterAsync(string username, string password, string email)
        {
            var data = new Dictionary<string, string>
            {
                { "username", username }, 
                { "email", email }, 
                { "password", password }
            };

            var resp = await PostAsync<LoginData>(UsersPath, data);

            if (!resp.Success)
            {
                return resp;
            }

            await SaveLoginStateAsync(resp);

            return resp;
        }

        public void Logout()
        {
            AuthenticationToken = null;
            refreshToken = null;
            _dispatcherHelper.RunAsync(() => CurrentUser = null);
            appSettingsHelper.Write("AudioticaCloudRefreshToken", null);
            appSettingsHelper.Write("AudioticaCloudUser", null);
            credentialHelper.DeleteCredentials("AudioticaCloud");
            _dispatcherHelper.RunAsync(() => RaisePropertyChanged(() => IsAuthenticated));
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
                { "name", card.Name }, 
                { "number", card.Number }, 
                { "expMonth", card.ExpMonth.ToString() }, 
                { "expYear", card.ExpYear.ToString() }, 
                { "cvc", card.Cvc }
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
    }
}