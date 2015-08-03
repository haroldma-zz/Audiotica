using Audiotica.Data.Spotify.Models;
using Audiotica.Web.Extensions;

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