using Audiotica.Web.Extensions;
using HtmlAgilityPack;

namespace Audiotica.Web.Http.Requets.MatchEngine.Mp3Pm
{
    public class Mp3MinerSearchRequest : RestObjectRequest<HtmlDocument>
    {
        public Mp3MinerSearchRequest(string page, string query)
        {
            this.Url("http://mp3miner.com/{page}/{query}").UrlParam("page", page).UrlParam("query", query).Get();
        }
    }
}