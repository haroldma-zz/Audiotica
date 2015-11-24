using Audiotica.Web.Extensions;
using HtmlAgilityPack;

namespace Audiotica.Web.Http.Requets.MatchEngine.MiMp3S
{
    public class MiMp3SSearchRequest : RestObjectRequest<HtmlDocument>
    {
        public MiMp3SSearchRequest(string query)
        {
            this.Url("http://www.mimp3s.com/{query}-mp3.html").UrlParam("query", query.Replace(" ", "-"));
        }
    }
}