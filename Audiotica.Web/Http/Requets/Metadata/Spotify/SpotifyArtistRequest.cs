using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.Metadata.Spotify.Models;

namespace Audiotica.Web.Http.Requets.Metadata.Spotify
{
    public class SpotifyArtistRequest : RestObjectRequest<FullArtist>
    {
        public SpotifyArtistRequest(string id)
        {
            this.ConfigureSpotify("artists/{id}").UrlParam("id", id);
        }
    }
}