using Audiotica.Web.Extensions;
using HtmlAgilityPack;

namespace Audiotica.Web.Http.Requets
{
    public class SongilySearchRequest : RestObjectRequest<HtmlDocument>
    {
        public SongilySearchRequest(string query)
        {
            //http://songily.com/mp3/download/1/boyfriend-justin-bieber.html
            this.Url("http://songily.com/mp3/download/{page}/{query}.html")
                .UrlParam("query", query.Replace("  ", " ").Replace(" ", "-"))
                .UrlParam("page", 1)
                .Get();
        }

        public SongilySearchRequest Page(int page)
        {
            return this.UrlParam("page", page);
        }
    }
}