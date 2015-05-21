using Audiotica.Web.Extensions;
using Audiotica.Web.Models.Meile;

namespace Audiotica.Web.Http.Requets
{
    public class MeileDetailsRequest : RestObjectRequest<MeileDetailRoot>
    {
        public MeileDetailsRequest(string songId)
        {
            this.Url("http://www.meile.com/song/mult").QParam("songId", songId).Get();
        }
    }
}