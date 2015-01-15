#region

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Audiotica.Core.Utilities;
using Audiotica.Data.Model;
using Audiotica.Data.Model.AudioticaCloud;
using Newtonsoft.Json.Linq;

#endregion

namespace Audiotica.Data.Service.RunTime
{
    public class AudioticaService
    {
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

        private string _authenticationToken;

        public AudioticaService()
        {
            var cred = CredentialHelper.GetCredentials("AudioticaCloud");
            if (cred == null) return;

            cred.RetrievePassword();
            _authenticationToken = cred.Password;

            CurrentUser = AppSettingsHelper.Read<AudioticaUser>("AudioticaCloudUser");
        }

        public AudioticaUser CurrentUser { get; set; }

        public HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();

            var token = ":" + AppToken;
            token = _authenticationToken ?? Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            if (_authenticationToken != null)
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
                var httpData = await json.DeserializeAsync<BaseAudioticaResponse<T>>().ConfigureAwait(false) ?? new BaseAudioticaResponse<T>();

                httpData.Success = resp.IsSuccessStatusCode;

                return httpData;
            }
        }

        private async Task<BaseAudioticaResponse> PostAsync(string url, Dictionary<string, string> data)
        {
            return await PostAsync<object>(url, data).ConfigureAwait(false) as BaseAudioticaResponse;
        }

        private async Task<BaseAudioticaResponse<T>> PostAsync<T>(string url, Dictionary<string, string> data)
        {
            using (var client = CreateHttpClient())
            {
                using (var content = new FormUrlEncodedContent(data))
                {
                    var resp = await client.PostAsync(url, content).ConfigureAwait(false);
                    var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var httpData = await json.DeserializeAsync<BaseAudioticaResponse<T>>().ConfigureAwait(false) ?? new BaseAudioticaResponse<T>();

                    httpData.Success = resp.IsSuccessStatusCode;

                    return httpData;
                }
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

            var resp = await PostAsync<LoginResponse>(TokenPath, data);

            if (!resp.Success)
                return resp;

            _authenticationToken = resp.Data.AuthenticationToken;
            CurrentUser = resp.Data.User;

            AppSettingsHelper.WriteAsJson("AudioticaCloudUser", CurrentUser);
            CredentialHelper.SaveCredentials("AudioticaCloud", resp.Data.User.Id,
                resp.Data.AuthenticationToken);

            return resp as BaseAudioticaResponse;
        }

        public async Task<BaseAudioticaResponse> RegisterAsync(string username, string password, string email)
        {
            var data = new Dictionary<string, string>
            {
                {"username", username},
                {"email", email},
                {"password", password}
            };

            var resp = await PostAsync(UsersPath, data);
            return resp;
        }

        public async Task<BaseAudioticaResponse<AudioticaUser>> GetProfile()
        {
            var resp = await GetAsync<AudioticaUser>(UsersPath);

            if (resp.Success)
            {
                //keping the user object updated
                CurrentUser = resp.Data;
                AppSettingsHelper.WriteAsJson("AudioticaCloudUser", CurrentUser);
            }

            return resp;
        }

        public async Task<BaseAudioticaResponse<List<WebSong>>> GetMatchesAsync(string title, string artist, int limit = 1)
        {
            var resp = await GetAsync<List<WebSong>>(string.Format(MatchPath, 
                Uri.EscapeDataString(title), Uri.EscapeDataString(artist), limit));
            return resp;
        }
    }
}