using Audiotica.Web.Extensions;
using Audiotica.Web.Models.SoundCloud;

namespace Audiotica.Web.Http.Requets
{
    public class SoundCloudSearchRequest : RestObjectRequest<SoundCloudRoot>
    {
        public SoundCloudSearchRequest(string clientId, string query)
        {
            this.Url("https://api.soundcloud.com/search/sounds").QParam("client_id", clientId).QParam("q", query).Get();
        }

        public SoundCloudSearchRequest Limit(int limit)
        {
            return this.QParam("limit", limit);
        }
    }
}