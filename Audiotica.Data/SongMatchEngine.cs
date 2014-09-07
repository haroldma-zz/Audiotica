#region License

// Copyright (c) 2014 Harry
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

#endregion

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

namespace Audiotica.Data
{
    public static class SongMatchEngine
    {
        //AKA music.163.com
        private const string NeteaseSearchApi = "http://music.163.com/api/search/get/web";
        private const string NeteaseDetailApi = "http://music.163.com/api/song/detail/?ids=%5B{0}%5D";

        public static async Task<string> GetUrlMatch(string title, string artist)
        {
            //TODO [Harry,20140906] BETA1 only using netease, implement for next beta more services.

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

                    if (parseResp.result.songs == null) return null;

                    var match =
                        parseResp.result.songs.FirstOrDefault(
                            s => String.Equals(s.name, title, StringComparison.CurrentCultureIgnoreCase) &&
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