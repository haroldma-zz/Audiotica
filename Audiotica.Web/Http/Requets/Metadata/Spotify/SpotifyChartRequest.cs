using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.Metadata.Spotify.Models;

namespace Audiotica.Web.Http.Requets.Metadata.Spotify
{
    public class SpotifyChartRequest : RestObjectRequest<SpotifyChartsResponse>
    {
        public SpotifyChartRequest()
        {
            this.Url("http://charts.spotify.com/api/tracks/{type}/{country}/{time}/latest")
                .MostStreamed().Time("weekly").Country("US");
        }

        public SpotifyChartRequest MostStreamed()
        {
            return this.UrlParam("type", "most_streamed");
        }

        public SpotifyChartRequest Viral()
        {
            return this.UrlParam("type", "viral");
        }

        public SpotifyChartRequest Time(string time)
        {
            return this.UrlParam("time", time);
        }

        public SpotifyChartRequest Country(string country)
        {
            return this.UrlParam("country", country);
        }
    }
}