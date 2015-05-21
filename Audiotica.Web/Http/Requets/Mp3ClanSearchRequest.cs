using Audiotica.Web.Extensions;
using HtmlAgilityPack;

namespace Audiotica.Web.Http.Requets
{
    public class Mp3ClanSearchRequest : RestObjectRequest<HtmlDocument>
    {
        public Mp3ClanSearchRequest(string query)
        {
            this.Url("http://mp3clan.com/mp3_source.php").Param("ser", query).Ajax().Post();
        }

        public Mp3ClanSearchRequest Page(int page)
        {
            return this.Param("page", page);
        }
    }
}