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

                    if (song.artwork_url.IndexOf('?') > -1)
                        song.artwork_url = song.artwork_url.Remove(song.artwork_url.LastIndexOf('?'));

                    song.artwork_url = song.artwork_url.Replace("large", "t500x500");
                }

                return parseResp.collection.FirstOrDefault().stream_url;
            }
        }
    }
}