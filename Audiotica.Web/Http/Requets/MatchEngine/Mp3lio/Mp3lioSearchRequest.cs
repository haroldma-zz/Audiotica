using Audiotica.Web.Extensions;
using HtmlAgilityPack;

namespace Audiotica.Web.Http.Requets.MatchEngine.Mp3lio
{
    public class Mp3lioSearchRequest : RestObjectRequest<HtmlDocument>
    {
        public Mp3lioSearchRequest(string query)
        {
            this.Url("http://mp3lio.com/{query}").UrlParam("query", query.Replace(" ", "-")).Get();
        }
    }
}