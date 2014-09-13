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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Audiotica.Core.Utilities;
using Audiotica.Data.Model.SoundCloud;

#endregion

namespace Audiotica.Data.Mp3Providers
{
    public class SoundCloudProvider : IMp3Provider
    {
        private const string SearchUrl = "https://api.soundcloud.com/search/sounds?client_id={0}&limit={1}&q={2}";

        public async Task<string> GetMatch(string title, string artist)
        {
            var url = string.Format(SearchUrl, ApiKeys.SoundCloudId, 5, title + " " + artist);

            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(new Uri(url));

                //ThrowIfError(resp);

                var json = await resp.Content.ReadAsStringAsync();
                var parseResp = await json.DeserializeAsync<SoundCloudRoot>();

                //Remove those that can't be stream
                parseResp.collection.RemoveAll(p => p.stream_url == null);

                foreach (var song in parseResp.collection)
                {
                    if (song.stream_url.Contains("soundcloud") && !song.stream_url.Contains("client_id"))
                        song.stream_url += "?client_id=" + ApiKeys.SoundCloudId;

                    if (string.IsNullOrEmpty(song.artwork_url))
                        song.artwork_url = song.user.avatar_url;

                    song.artwork_url = song.artwork_url.Remove(song.artwork_url.LastIndexOf('?'))
                        .Replace("large", "t500x500");
                }

                return parseResp.collection.FirstOrDefault().stream_url;
            }
        }
    }
}