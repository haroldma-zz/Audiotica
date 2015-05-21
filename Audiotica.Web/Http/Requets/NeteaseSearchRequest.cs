using Audiotica.Web.Extensions;
using Audiotica.Web.Models.Netease;

namespace Audiotica.Web.Http.Requets
{
    public class NeteaseSearchRequest : RestObjectRequest<NeteaseRoot>
    {
        public NeteaseSearchRequest(string query)
        {
            this.Url("http://music.163.com/api/search/suggest/web")
                .Referer("http://music.163.com").Param("s", query).Post();
        }
    }
}