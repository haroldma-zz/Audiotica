using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.MatchEngine.Meile.Models;

namespace Audiotica.Web.Http.Requets.MatchEngine.Meile
{
    public class MeileDetailsRequest : RestObjectRequest<MeileDetailRoot>
    {
        public MeileDetailsRequest(string songId)
        {
            this.Url("http://www.meile.com/song/mult").QParam("songId", songId).Get();
        }
    }
}