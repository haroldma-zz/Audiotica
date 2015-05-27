using Audiotica.Data.Spotify.Models;
using Audiotica.Web.Extensions;

namespace Audiotica.Web.Http.Requets.Metadata.Spotify
{
    public class SpotifySearchRequest : RestObjectRequest<SearchItem>
    {
        public SpotifySearchRequest(string query, string type)
        {
            this.Url("https://api.spotify.com/v1/search")
                .QParam("type", type).QParam("market", "US").QParam("q", query).Get();
        }

        public SpotifySearchRequest Limit(int limit)
        {
            return this.QParam("limit", limit);
        }

        public SpotifySearchRequest Offset(int offset)
        {
            return this.QParam("offset", offset);
        }

        public SpotifySearchRequest Market(string market)
        {
            return this.QParam("market", market);
        }
    }
}