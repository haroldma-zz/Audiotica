using Audiotica.Data.Spotify.Models;
using Audiotica.Web.Extensions;

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