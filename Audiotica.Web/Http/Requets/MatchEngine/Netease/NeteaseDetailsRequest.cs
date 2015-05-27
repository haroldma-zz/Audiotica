using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.MatchEngine.Netease.Models;

namespace Audiotica.Web.Http.Requets.MatchEngine.Netease
{
    public class NeteaseDetailsRequest : RestObjectRequest<NeteaseDetailRoot>
    {
        public NeteaseDetailsRequest(int songId)
        {
            this.Url("http://music.163.com/api/song/detail/").QParam("ids", $"[{songId}]").Get();
        }
    }
}