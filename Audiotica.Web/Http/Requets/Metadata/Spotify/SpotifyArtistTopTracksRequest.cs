using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.Metadata.Spotify.Models;

namespace Audiotica.Web.Http.Requets.Metadata.Spotify
{
    public class SpotifyArtistTopTracksRequest : RestObjectRequest<Paging<SimpleTrack>>
    {
        public SpotifyArtistTopTracksRequest(string id)
        {
            this.ConfigureSpotify("artists/{id}/top-tracks").UrlParam("id", id).Country("US").Offset(0).Limit(50);
        }

        public SpotifyArtistTopTracksRequest Offset(int offset)
        {
            return this.QParam("offset", offset);
        }

        public SpotifyArtistTopTracksRequest Limit(int limit)
        {
            return this.QParam("limit", limit);
        }

        public SpotifyArtistTopTracksRequest Country(string country)
        {
            return this.QParam("country", country);
        }
    }
}