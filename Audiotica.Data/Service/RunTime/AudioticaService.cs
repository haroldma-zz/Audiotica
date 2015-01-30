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
        private readonly ICredentialHelper credentialHelper;
        private readonly IAppSettingsHelper appSettingsHelper;
#if DEBUG
        private const string BaseApiPath = "http://localhost:48065/api/";
        private const string AppToken = "LOCALTESTING";
#else
        private const string BaseApiPath = "https://audiotica-cloud.azure-mobile.net/api/";
        private const string AppToken = "AypzKLKRIDPGkXXzCGYGqjJNliXTwp74";
#endif

        private const string UsersPath = BaseApiPath + "users";
        private const string TokenPath = UsersPath + "/token";
        private const string MatchPath = BaseApiPath + "match?title={0}&artist={1}&limit={2}";

        private string authenticationToken;

        public AudioticaService(ICredentialHelper credentialHelper, IAppSettingsHelper appSettingsHelper)
        {
            this.credentialHelper = credentialHelper;
            this.appSettingsHelper = appSettingsHelper;

            var cred = this.credentialHelper.GetCredentials("AudioticaCloud");
            if (cred == null) return;

            authenticationToken = cred.GetPassword();

            CurrentUser = this.appSettingsHelper.ReadJsonAs<AudioticaUser>("AudioticaCloudUser");
            refreshToken = this.appSettingsHelper.Read("AudioticaCloudRefreshToken");
        }

        private string refreshToken;
        public AudioticaUser CurrentUser { get; private set; }

        public bool IsAuthenticated
        {
            get { return authenticationToken != null; }
        }

        public HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();

            var token = ":" + AppToken;
            token = authenticationToken ?? Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            if (authenticationToken != null)
                httpClient.DefaultRequestHeaders.Add("X-ZUMO-AUTH", token);
            else
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);

            return httpClient;
        }

        #region Helper methods

        private async Task<BaseAudioticaResponse<T>> GetAsync<T>(string url)
        {
            using (var client = CreateHttpClient())
            {
                var resp = await client.GetAsync(url).ConfigureAwait(false);
                var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                var httpData = await json.DeserializeAsync<BaseAudioticaResponse<T>>().ConfigureAwait(false) ??
                               new BaseAudioticaResponse<T>();

                httpData.StatusCode = resp.StatusCode;
                httpData.Success = resp.IsSuccessStatusCode;

                if (resp.StatusCode != HttpStatusCode.Unauthorized)
                    return httpData;

                if (string.IsNullOrEmpty(refreshToken))
                    return httpData;

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
            var data = new Dictionary<string, string>
            {
                {"RefreshToken", refreshToken}
            };

            Logout();

            var resp = await LoginAsync(data);
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
                var httpData = await json.DeserializeAsync<BaseAudioticaResponse<T>>().ConfigureAwait(false) ??
                               new BaseAudioticaResponse<T>();

                httpData.Success = resp.IsSuccessStatusCode;
                httpData.StatusCode = resp.StatusCode;

                if (resp.StatusCode != HttpStatusCode.Unauthorized)
                    return httpData;

                if (string.IsNullOrEmpty(refreshToken))
                    return httpData;

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

        #endregion

        public async Task<BaseAudioticaResponse> LoginAsync(string username, string password)
        {
            var data = new Dictionary<string, string>
            {
                {"username", username},
                {"password", password}
            };

            return await LoginAsync(data);
        }

        private async Task<BaseAudioticaResponse> LoginAsync(Dictionary<string, string> data)
        {
            var resp = await PostAsync<LoginData>(TokenPath, data);

            if (!resp.Success)
                return resp;

            SaveLoginState(resp);

            return resp;
        }

        private void SaveLoginState(BaseAudioticaResponse<LoginData> resp)
        {
            authenticationToken = resp.Data.AuthenticationToken;
            CurrentUser = resp.Data.User;

            refreshToken = resp.Data.RefreshToken;
            appSettingsHelper.Write("AudioticaCloudRefreshToken", refreshToken);
            appSettingsHelper.WriteAsJson("AudioticaCloudUser", CurrentUser);
            credentialHelper.SaveCredentials("AudioticaCloud", resp.Data.User.Id,
                resp.Data.AuthenticationToken);
            RaisePropertyChanged(() => IsAuthenticated);
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
                return resp;

            SaveLoginState(resp);

            return resp;
        }

        public void Logout()
        {
            authenticationToken = null;
            refreshToken = null;
            CurrentUser = null;
            appSettingsHelper.Write("AudioticaCloudRefreshToken", null);
            appSettingsHelper.Write("AudioticaCloudUser", null);
            credentialHelper.DeleteCredentials("AudioticaCloud");
            RaisePropertyChanged(() => IsAuthenticated);
        }

        public async Task<BaseAudioticaResponse<AudioticaUser>> GetProfileAsync()
        {
            var resp = await GetAsync<AudioticaUser>(UsersPath + "/me");

            if (!resp.Success) return resp;

            //keping the user object updated
            CurrentUser = resp.Data;
            appSettingsHelper.WriteAsJson("AudioticaCloudUser", CurrentUser);

            return resp;
        }

        public async Task<BaseAudioticaResponse<List<WebSong>>> GetMatchesAsync(string title, string artist,
                                                                                int limit = 1)
        {
            var resp = await GetAsync<List<WebSong>>(string.Format(MatchPath,
                Uri.EscapeDataString(title), Uri.EscapeDataString(artist), limit));
            return resp;
        }
    }
}