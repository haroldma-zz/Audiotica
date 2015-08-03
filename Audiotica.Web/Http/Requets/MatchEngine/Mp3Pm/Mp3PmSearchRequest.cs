using Audiotica.Web.Extensions;
using HtmlAgilityPack;

namespace Audiotica.Web.Http.Requets.MatchEngine.Mp3Pm
{
    public class Mp3PmSearchRequest : RestObjectRequest<HtmlDocument>
    {
        public Mp3PmSearchRequest(string query)
        {
            this.Url("http://mp3pm.com/s/f/{query}/").UrlParam("query", query).Get();
        }
    }
}