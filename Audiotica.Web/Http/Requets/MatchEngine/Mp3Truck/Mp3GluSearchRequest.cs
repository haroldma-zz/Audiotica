using Audiotica.Web.Extensions;
using HtmlAgilityPack;

namespace Audiotica.Web.Http.Requets.MatchEngine.Mp3Truck
{
    public class Mp3TruckSearchRequest : RestObjectRequest<HtmlDocument>
    {
        public Mp3TruckSearchRequest(string query)
        {
            this.Url("http://mp3glu.com/search.php")
                .QParam("q", query)
                .Post();
        }
    }
}