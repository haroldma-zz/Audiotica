using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.Metadata.Deezer.Models;
using Audiotica.Web.Models;
using Newtonsoft.Json.Linq;

namespace Audiotica.Web.Http.Requets.Metadata.Deezer
{
    public class DeezerSearchRequest<T> : RestObjectRequest<DeezerPageResponse<T>>
    {
        public DeezerSearchRequest(string query)
        {
            this.Url("http://api.deezer.com/search/{type}").Type(WebResults.Type.Song).Param("q", query);
        }

        public DeezerSearchRequest<T> Type(WebResults.Type type)
        {
            return this.UrlParam("type", type.ToString().ToLower().Replace("song", "track"));
        }

        public DeezerSearchRequest<T> Limit(int limit)
        {
            return this.Param("limit", limit);
        }

        public DeezerSearchRequest<T> Offset(int offset)
        {
            return this.Param("index", offset);
        }
    }
}