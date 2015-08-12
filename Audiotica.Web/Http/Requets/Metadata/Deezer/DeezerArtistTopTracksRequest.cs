using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.Metadata.Deezer.Models;

namespace Audiotica.Web.Http.Requets.Metadata.Deezer
{
    public class DeezerArtistTopTracksRequest : RestObjectRequest<DeezerPageResponse<DeezerSong>>
    {
        public DeezerArtistTopTracksRequest(int id)
        {
            this.Url("http://api.deezer.com/artist/{id}/top").UrlParam("id", id);
        }

        public DeezerArtistTopTracksRequest Limit(int limit)
        {
            return this.Param("limit", limit);
        }

        public DeezerArtistTopTracksRequest Offset(int offset)
        {
            return this.Param("index", offset);
        }
    }
}