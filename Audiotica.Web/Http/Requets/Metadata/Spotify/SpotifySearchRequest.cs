using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.Metadata.Spotify.Models;
using Audiotica.Web.Models;

namespace Audiotica.Web.Http.Requets.Metadata.Spotify
{
    public class SpotifySearchRequest : RestObjectRequest<SearchItem>
    {
        public SpotifySearchRequest(string query)
        {
            this.ConfigureSpotify("search").Type(WebResults.Type.Song).QParam("q", query);
        }

        public SpotifySearchRequest Type(WebResults.Type type)
        {
            return this.Param("type", type.ToString().ToLower().Replace("song", "track"));
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