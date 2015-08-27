using Audiotica.Web.Extensions;
using Newtonsoft.Json.Linq;

namespace Audiotica.Web.Http.Requets.MatchEngine.Pleer
{
    public class PleerSearchRequest : RestObjectRequest<JToken>
    {
        public PleerSearchRequest(string query)
        {
            this.Url("http://pleer.com/search")
                .QParam("onlydata", true).QParam("quality", "best")
                .QParam("sort_mode", 0).QParam("sort_by", 0)
                .QParam("page", 1).QParam("q", query)
                .Ajax().Get();
        }

        public PleerSearchRequest Page(int page)
        {
            return this.QParam("page", page);
        }
    }
}