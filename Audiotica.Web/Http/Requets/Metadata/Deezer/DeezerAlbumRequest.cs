using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.Metadata.Deezer.Models;

namespace Audiotica.Web.Http.Requets.Metadata.Deezer
{
    public class DeezerAlbumRequest : RestObjectRequest<DeezerAlbum>
    {
        public DeezerAlbumRequest(string id)
        {
            this.Url("http://api.deezer.com/album/{id}").UrlParam("id", id);
        }
    }
}