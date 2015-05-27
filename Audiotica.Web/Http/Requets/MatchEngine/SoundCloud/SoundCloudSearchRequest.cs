using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.MatchEngine.SoundCloud.Models;

namespace Audiotica.Web.Http.Requets.MatchEngine.SoundCloud
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