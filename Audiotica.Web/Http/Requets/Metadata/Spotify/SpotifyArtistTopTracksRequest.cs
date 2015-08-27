using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.Metadata.Spotify.Models;

namespace Audiotica.Web.Http.Requets.Metadata.Spotify
{
    public class SpotifyArtistTopTracksRequest : RestObjectRequest<PlainTrackResponse>
    {
        public SpotifyArtistTopTracksRequest(string id)
        {
            this.ConfigureSpotify("artists/{id}/top-tracks").UrlParam("id", id).Country("US");
        }

        public SpotifyArtistTopTracksRequest Country(string country)
        {
            return this.QParam("country", country);
        }
    }
}