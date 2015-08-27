using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.Metadata.Deezer.Models;

namespace Audiotica.Web.Http.Requets.Metadata.Deezer
{
    public class DeezerArtistRequest : RestObjectRequest<DeezerArtist>
    {
        public DeezerArtistRequest(string id)
        {
            this.Url("http://api.deezer.com/artist/{id}").UrlParam("id", id);
        }
    }
}