using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.MatchEngine.Netease.Models;

namespace Audiotica.Web.Http.Requets.MatchEngine.Netease
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