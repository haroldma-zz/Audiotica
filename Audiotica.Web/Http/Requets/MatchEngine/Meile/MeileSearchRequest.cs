using Audiotica.Web.Extensions;
using HtmlAgilityPack;

namespace Audiotica.Web.Http.Requets.MatchEngine.Meile
{
    public class MeileSearchRequest : RestObjectRequest<HtmlDocument>
    {
        public MeileSearchRequest(string query)
        {
            this.Url("http://www.meile.com/search").QParam("q", query).Get();
        }

        public MeileSearchRequest Limit(int limit)
        {
            return this.Param("count", limit);
        }
    }
}