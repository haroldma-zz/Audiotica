#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Audiotica.Core.Exceptions;
using Audiotica.Core.Utilities;
using Audiotica.Data.Model;

#endregion

namespace Audiotica.Data.Mp3Providers
{
    public class NeteaseProvider : IMp3Provider
    {
        //AKA music.163.com
        private const string NeteaseSearchApi = "http://music.163.com/api/search/get/web";
        private const string NeteaseDetailApi = "http://music.163.com/api/song/detail/?ids=%5B{0}%5D";

        public async Task<string> GetMatch(string title, string artist)
        {
            using (var client = new HttpClient())
            {
                //Setting referer (163 doesn't work without it)
                client.DefaultRequestHeaders.Referrer = new Uri("http://music.163.com");

                //Lets go ahead and search for a match
                var dict = new Dictionary<string, string>
                {
                    {"s", title + " " + artist},
                    {"type", "1"},
                    {"offset", "0"},
                    {"limit", "5"},
                    {"total", "true"}
                };

                using (var data = new FormUrlEncodedContent(dict))
                {
                    var resp = await client.PostAsync(NeteaseSearchApi, data);
                    var json = await resp.Content.ReadAsStringAsync();
                    var parseResp = await json.DeserializeAsync<NeteaseRoot>();
                    if (!resp.IsSuccessStatusCode) throw new NetworkException();

                    if (parseResp.result == null || parseResp.result.songs == null) return null;

                    var match =
                        parseResp.result.songs.FirstOrDefault(
                            s => String.Equals(s.name, title, StringComparison.CurrentCultureIgnoreCase) ||
                                 s.artists.Count(
                                     p => String.Equals(p.name, artist, StringComparison.CurrentCultureIgnoreCase)) != 0);

                    if (match == null) return null;

                    //Now lets get the mp3 urls for it
                    var detailResp = await client.GetAsync(string.Format(NeteaseDetailApi, match.id));
                    var detailJson = await detailResp.Content.ReadAsStringAsync();
                    var detailParseResp = await detailJson.DeserializeAsync<NeteaseDetailRoot>();
                    if (!detailResp.IsSuccessStatusCode) throw new NetworkException();

                    if (detailParseResp.songs == null) return null;

                    var detailMatch = detailParseResp.songs.FirstOrDefault();

                    return detailMatch.mp3Url;
                }
            }
        }
    }
}