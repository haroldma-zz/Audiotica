using System;
using System.Threading.Tasks;
using Audiotica.Web.Extensions;
using Newtonsoft.Json.Linq;

namespace Audiotica.Web.Http.Requets.Metadata.Google
{
    internal abstract class MusixMatchAuthRequest : MusixMatchBaseRequest
    {
        protected MusixMatchAuthRequest(string path) : base(path)
        {
        }

        protected static string SessionUserToken { get; set; }

        public override async Task<RestResponse<JToken>> ToResponseAsync()
        {
            if (SessionUserToken == null)
            {
                var tokenResponse = await new MusixMatchGetTokenRequest().ToResponseAsync();
                if (tokenResponse.IsSuccessStatusCode)
                {
                    SessionUserToken = tokenResponse.Data["message"]["body"].Value<string>("user_token");
                }
                else
                {
                    throw new Exception("Failed to obtained user token.");
                }
            }

            this.QParam("usertoken", SessionUserToken);
            return await base.ToResponseAsync();
        }
    }
}