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

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Audiotica.Core.Utilities;
using Audiotica.Data.Model;
using HtmlAgilityPack;

#endregion

namespace Audiotica.Data.Mp3Providers
{
    public class MeileProvider : IMp3Provider
    {
        private const string SearchUrl = "http://www.meile.com/search?type=&q={0}";
        private const string DetailUrl = "http://www.meile.com/song/mult?songId={0}";

        public async Task<string> GetMatch(string title, string artist)
        {
            var url = string.Format(SearchUrl, WebUtility.UrlEncode(title + " " + artist));

            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(url);
                var html = await resp.Content.ReadAsStringAsync();

                //Meile has no search api, so we go to old school web page scrapping
                //using HtmlAgilityPack this is very easy
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                //Get the hyperlink node with the class='name'
                var songNameNode = doc.DocumentNode.Descendants("a")
                    .FirstOrDefault(p => p.Attributes.Contains("class") && p.Attributes["class"].Value == "name");

                if (songNameNode == null) return null;

                //in it there is an attribute that contains the url to the song
                var songId = songNameNode.Attributes["href"].Value;

                if (!songId.Contains("/song/")) return null;

                //Remove some stuff from it and we got the id.
                songId = songId.Replace("/song/", "");

                //Now we use the detail api to get the mp3 url
                return await GetMp3UrlFor(songId, client);
            }
        }

        private async Task<string> GetMp3UrlFor(string songId, HttpClient client)
        {
            var url = string.Format(DetailUrl, songId);

            var resp = await client.GetAsync(url);
            var json = await resp.Content.ReadAsStringAsync();
            var parseResp = await json.DeserializeAsync<MeileDetailRoot>();

            return parseResp.values.songs.FirstOrDefault().mp3;
        }
    }
}