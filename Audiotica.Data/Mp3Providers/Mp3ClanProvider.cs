#region

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Audiotica.Core.Utilities;
using Audiotica.Data.Model;

#endregion

namespace Audiotica.Data.Mp3Providers
{
    public class Mp3ClanProvider : IMp3Provider
    {
        private const string SearchUrl = "http://mp3clan.com/app/mp3Search.php?q={0}&count={1}";

        public async Task<string> GetMatch(string title, string artist)
        {
            var url = string.Format(SearchUrl, WebUtility.UrlEncode(title + " " + artist), "5");

            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(url);
                var json = await resp.Content.ReadAsStringAsync();
                var parseResp = await json.DeserializeAsync<Mp3ClanRoot>();

                if (parseResp == null || parseResp.response == null || !resp.IsSuccessStatusCode) return null;

                var matches = parseResp.response.Where(
                            s => s.title.Contains(title) &&
                                 s.artist.Contains(artist));

                // is this song supposed to be a remix?
                var isSupposedToBeMix = title.ToLower().Contains("mix");

                if (matches == null) return null;

                // get the first match that is a mix (if is supposed to) or not
                var match = matches.FirstOrDefault(p =>
                {
                    var isMix = p.title.ToLower().Contains("mix");
                    return isSupposedToBeMix && isMix || !isSupposedToBeMix && !isMix;
                });

                return match != null ? match.url : null;
            }
        }
    }
}