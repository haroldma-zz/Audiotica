using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.Metadata.Spotify.Models;

namespace Audiotica.Web.Http.Requets.Metadata.Spotify
{
    public class SpotifyArtistAlbumsRequest : RestObjectRequest<Paging<SimpleAlbum>>
    {
        public SpotifyArtistAlbumsRequest(string id)
        {
            this.ConfigureSpotify("artists/{id}/albums").UrlParam("id", id).Country("US").Offset(0).Limit(50);
        }

        public SpotifyArtistAlbumsRequest Offset(int offset)
        {
            return this.QParam("offset", offset);
        }

        public SpotifyArtistAlbumsRequest Limit(int limit)
        {
            return this.QParam("limit", limit);
        }

        public SpotifyArtistAlbumsRequest Country(string country)
        {
            return this.QParam("country", country);
        }
    }
}