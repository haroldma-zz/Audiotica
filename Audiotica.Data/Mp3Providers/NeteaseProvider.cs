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
        private const string NeteaseSuggestApi = "http://music.163.com/api/search/suggest/web?csrf_token=";
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
                    {"limit", "5"}
                };

                using (var data = new FormUrlEncodedContent(dict))
                {
                    var resp = await client.PostAsync(NeteaseSuggestApi, data);
                    var json = await resp.Content.ReadAsStringAsync();
                    var parseResp = await json.DeserializeAsync<NeteaseRoot>();
                    if (!resp.IsSuccessStatusCode) throw new NetworkException();

                    if (parseResp == null || parseResp.result == null || parseResp.result.songs == null) return null;

                    // get all possible matches
                    var matches =
                        parseResp.result.songs.Where(
                            s => s.name.Contains(title) &&
                                 s.artists.Count(
                                     p => p.name.Contains(artist)) != 0);

                    // is this song supposed to be a remix?
                    var isSupposedToBeMix = title.Contains("mix");

                    if (matches == null) return null;

                    // get the first match that is a mix (if is supposed to) or not
                    var match = matches.FirstOrDefault(p =>
                    {
                        var isMix = p.name.Contains("mix");
                        return isSupposedToBeMix || !isMix;
                    });

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