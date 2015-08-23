using Audiotica.Web.Extensions;
using HtmlAgilityPack;

namespace Audiotica.Web.Http.Requets.MatchEngine.Mp3Truck
{
    public class Mp3TruckSearchRequest : RestObjectRequest<HtmlDocument>
    {
        public Mp3TruckSearchRequest(string query)
        {
            this.Url("https://mp3truck.net/search.php")
                .Param("sort", "relevance")
                .Param("p", 1)
                .QParam("q", query)
                .Post();
        }

        public Mp3TruckSearchRequest Page(int page)
        {
            return this.Param("p", page);
        }

        public Mp3TruckSearchRequest Sort(string sort)
        {
            return this.Param("sort", sort);
        }
    }
}