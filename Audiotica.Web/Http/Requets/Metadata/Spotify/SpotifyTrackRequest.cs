using Audiotica.Data.Spotify.Models;
using Audiotica.Web.Extensions;

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