#region

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Audiotica.Core.Utilities;
using Newtonsoft.Json;
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

        public AudioticaUser CurrentUser { get; set; }

        public HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();

            var token = ":" + AppToken;
            if (CurrentUser != null)
                token = CurrentUser.AuthenticationToken;
            token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            if (CurrentUser != null)
                httpClient.DefaultRequestHeaders.Add("X-ZUMO-AUTH", token);
            else
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);

            return httpClient;
        }

        private async Task<HttpResult<T>> PostAsync<T>(string url, Dictionary<string, string> data)
        {
            using (var client = CreateHttpClient())
            {
                using (var content = new FormUrlEncodedContent(data))
                {
                    var resp = await client.PostAsync(url, content);
                    var json = await resp.Content.ReadAsStringAsync();
                    var httpData = await json.DeserializeAsync<T>();
                    return new HttpResult<T>
                    {
                        Data = httpData,
                        Success = resp.IsSuccessStatusCode
                    };
                }
            }
        }

        public async Task<BaseAudioticaResponse<bool>> LoginAsync(string username, string password)
        {
            var data = new Dictionary<string, string>
            {
                {"username", username},
                {"password", password}
            };

            var resp = await PostAsync<JToken>(TokenPath, data);

            if (!resp.Success)
                return new BaseAudioticaResponse<bool>
                {
                    Message = resp.Data == null ? null : resp.Data.Value<string>("message")
                };

            CurrentUser = new AudioticaUser
            {
                AuthenticationToken = resp.Data.Value<string>("authenticationToken"),
                UserId = resp.Data.SelectToken("user").Value<string>("userId")
            };
            CredentialHelper.SaveCredentials("AudioticaCloud", CurrentUser.UserId,
                CurrentUser.AuthenticationToken);

            return new BaseAudioticaResponse<bool>
            {
                Data = true
            };
        }

        public async Task<BaseAudioticaResponse<bool>> RegisterAsync(string username, string password, string email)
        {
            var data = new Dictionary<string, string>
            {
                {"username", username},
                {"email", email},
                {"password", password}
            };

            var resp = await PostAsync<JToken>(UsersPath, data);

            if (!resp.Success)
                return new BaseAudioticaResponse<bool>
                {
                    Message = resp.Data == null ? null : resp.Data.Value<string>("message")
                };

            return new BaseAudioticaResponse<bool>
            {
                Data = true
            };
        }
    }

    public class AudioticaUser
    {
        public string AuthenticationToken { get; set; }

        public string UserId { get; set; }

        public string Username { get; set; }
    }

    public class BaseAudioticaResponse<T> : BaseAudioticaResponse
    {
        [JsonProperty(PropertyName = "data")]
        public T Data { get; set; }
    }

    public class BaseAudioticaResponse
    {
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }

    public class HttpResult<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
    }
}