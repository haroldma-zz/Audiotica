using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.Metadata.Spotify.Models;

namespace Audiotica.Web.Http.Requets.Metadata.Spotify
{
    public class SpotifyAlbumRequest : RestObjectRequest<FullAlbum>
    {
        public SpotifyAlbumRequest(string id)
        {
            this.ConfigureSpotify("albums/{id}").UrlParam("id", id);
        }
    }
}