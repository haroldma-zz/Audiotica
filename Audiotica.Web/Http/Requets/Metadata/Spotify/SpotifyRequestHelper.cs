using Audiotica.Web.Extensions;

namespace Audiotica.Web.Http.Requets.Metadata.Spotify
{
    internal static class SpotifyRequestHelper
    {
        public const string BasePath = "https://api.spotify.com/v1/";

        public static T ConfigureSpotify<T>(this T request, string path)
            where T : RestRequest
        {
            request.DeserializeOnError = true;
            return request.Url(BasePath + path).QParam("market", "US").Get();
        }
    }
}