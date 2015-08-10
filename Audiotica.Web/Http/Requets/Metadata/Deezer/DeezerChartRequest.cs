using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.Metadata.Deezer.Models;
using Audiotica.Web.Models;

namespace Audiotica.Web.Http.Requets.Metadata.Deezer
{
    public class DeezerChartRequest<T> : RestObjectRequest<DeezerPageResponse<T>>
    {
        public DeezerChartRequest(WebResults.Type type)
        {
            this.Url("http://api.deezer.com/chart/0/{type}").Type(type);
        }

        public DeezerChartRequest<T> Type(WebResults.Type type)
        {
            return this.UrlParam("type", type.ToString().ToLower().Replace("song", "track") + "s");
        }

        public DeezerChartRequest<T> Limit(int limit)
        {
            return this.Param("limit", limit);
        }

        public DeezerChartRequest<T> Offset(int offset)
        {
            return this.Param("index", offset);
        }
    }
}