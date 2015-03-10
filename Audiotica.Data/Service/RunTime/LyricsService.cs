using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Audiotica.Data.Service.RunTime
{
    public class LyricsService
    {
        private const string MetroLyricsUrl =
            "http://api.metrolyrics.com/v1/get/fullbody/?title={0}&artist={1}&X-API-KEY=b84a4db3a6f9fb34523c25e43b387f1f56f987a5&format=json";

        public async Task<string> GetLyrics(string title, string artist)
        {
            var url = string.Format(MetroLyricsUrl, WebUtility.UrlEncode(title), WebUtility.UrlEncode(artist));
            using (var client = new HttpClient())
            {
                using (var resp = await client.GetAsync(url))
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    try
                    {
                        var o = JToken.Parse(json);
                        return o.Value<string>("song");
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }
    }
}