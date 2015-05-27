using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Web.Extensions;
using HtmlAgilityPack;

namespace Audiotica.Web.Http.Requets.MatchEngine.Mp3Skull
{
    public class Mp3SkullRequest : RestRequest
    {
        public Mp3SkullRequest()
        {
            this.Url("http://mp3skull.com/search_db.php").Get();
        }

        public Mp3SkullRequest Fckh(int token)
        {
            return this.QParam("fckh", token);
        }

        public async Task<RestResponse<HtmlDocument>> SearchAsync(string query)
        {
            return await this.QParam("q", query).Fetch<HtmlDocument>().DontMarshall();
        }
    }
}