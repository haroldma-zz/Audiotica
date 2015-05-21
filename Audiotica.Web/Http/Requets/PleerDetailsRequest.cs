using Audiotica.Web.Extensions;
using Newtonsoft.Json.Linq;

namespace Audiotica.Web.Http.Requets
{
    public class PleerDetailsRequest : RestObjectRequest<JToken>
    {
        public PleerDetailsRequest(string songId)
        {
            this.Url("http://pleer.com/site_api/files/get_url").Param("action", "download").Param("id", songId).Post();
        }
    }
}