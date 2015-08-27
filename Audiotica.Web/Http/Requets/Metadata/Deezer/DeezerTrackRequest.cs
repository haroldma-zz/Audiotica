using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.Metadata.Deezer.Models;

namespace Audiotica.Web.Http.Requets.Metadata.Deezer
{
    public class DeezerTrackRequest : RestObjectRequest<DeezerSong>
    {
        public DeezerTrackRequest(string id)
        {
            this.Url("http://api.deezer.com/track/{id}").UrlParam("id", id);
        }
    }
}