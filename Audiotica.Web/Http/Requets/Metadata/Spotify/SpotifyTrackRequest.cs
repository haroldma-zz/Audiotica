using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.Metadata.Spotify.Models;

namespace Audiotica.Web.Http.Requets.Metadata.Spotify
{
    public class SpotifyTrackRequest : RestObjectRequest<FullTrack>
    {
        public SpotifyTrackRequest(string id)
        {
            this.ConfigureSpotify("tracks/{id}").UrlParam("id", id);
        }
    }
}