using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.MatchEngine.AnyMaza.Model;

namespace Audiotica.Web.Http.Requets.MatchEngine.AnyMaza
{
    internal class AnyMazaSearchRequest : RestObjectRequest<GoogleSearchRoot>
    {
        public AnyMazaSearchRequest(string query)
        {
            this.Url("https://ajax.googleapis.com/ajax/services/search/web")
                .QParam("q", query + " site:AnyMaza.com").QParam("v", "1.0");
        }
    }
}