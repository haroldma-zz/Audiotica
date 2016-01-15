using System;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Extensions;
using Audiotica.Web.Extensions;
using Newtonsoft.Json.Linq;

namespace Audiotica.Web.Http.Requets.Metadata.Google
{
    internal abstract class MusixMatchBaseRequest : RestObjectRequest<JToken>
    {
        private const string AppId = "windows-8-cp-v1.0";
        private const string Secret = "secretsuper";

        protected MusixMatchBaseRequest(string path)
        {
            this.Url("https://apic.musixmatch.com/ws/1.1/" + path)
                .QParam("app_id", AppId)
                .QParam("format", "json");
        }

        public override Task<RestResponse<JToken>> ToResponseAsync()
        {
            this.QParam("signature",
                CalculateSignature(new Uri(
                    Url.CreateRequestUri(
                        QueryParams,
                        Param,
                        Request.Method.Method).ToUnaccentedText()).AbsolutePath));
            this.QParam("signature_protocol", "sha1");
            return base.ToResponseAsync();
        }

        private string CalculateSignature(string path)
        {
            var message = path
                + string.Format("{0:yyyy}{0:MM}{0:dd}", DateTime.UtcNow);
            return SHA.ComputeHMAC_SHA1(Secret.ToBytes(), message.ToBytes()).ToBase64();
        }
    }
}