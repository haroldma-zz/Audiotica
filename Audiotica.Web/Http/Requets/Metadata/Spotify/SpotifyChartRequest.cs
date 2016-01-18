using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.Metadata.Spotify.Models;

namespace Audiotica.Web.Http.Requets.Metadata.Spotify
{
    public class SpotifyChartRequest : RestObjectRequest<SpotifyChartsResponse>
    {
        public SpotifyChartRequest()
        {
            this.Url("https://spotifycharts.com/api/?limit={limit}&country={country}&recurrence={time}&date=latest&type={type}")
                .Regional().Time("weekly").Country("US");
        }

        public SpotifyChartRequest Regional()
        {
            return this.UrlParam("type", "regional");
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

        public SpotifyChartRequest Limit(int limit)
        {
            return this.UrlParam("limit", limit);
        }
    }
}