using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Audiotica.Core.Exceptions;
using Audiotica.Core.Utilities;
using Audiotica.Data.Model;
using Audiotica.Data.Model.SoundCloud;
using Audiotica.Data.Service.Interfaces;
using HtmlAgilityPack;

namespace Audiotica.Data.Service.RunTime
{
    public class Mp3SearchService : IMp3SearchService
    {
        private const string SoundCloudSearchUrl = "https://api.soundcloud.com/search/sounds?client_id={0}&limit={1}&q={2}";
        
        private const string Mp3ClanSearchUrl = "http://mp3clan.com/app/mp3Search.php?q={0}&count={1}";

        private const string MeileSearchUrl = "http://www.meile.com/search?type=&q={0}";
        private const string MeileDetailUrl = "http://www.meile.com/song/mult?songId={0}";

        private const string NeteaseSuggestApi = "http://music.163.com/api/search/suggest/web?csrf_token=";
        private const string NeteaseDetailApi = "http://music.163.com/api/song/detail/?ids=%5B{0}%5D";

        private string CreateQuery(string title, string artist, string album, bool urlEncode = true)
        {
            var query = ((title + " " + artist).Trim() + album).Trim();
            return urlEncode ? WebUtility.UrlEncode(query) : query;
        }

        private List<WebSong> FilterByTypeAndMatch(IEnumerable<WebSong> songs, string title, string artist)
        {
            return songs.Where(p =>
            {
                var isCorrectType = IsCorrectType(title, p.Title, "mix")
                    && IsCorrectType(title, p.Title, "cover")
                    && IsCorrectType(title, p.Title, "live")
                    && IsCorrectType(title, p.Title, "acoustic");

                var isCorrectTitle = p.Title.ToLower().Contains(title.ToLower());
                var isCorrectArtist = p.Artist != null 
                    ? p.Artist.ToLower().Contains(artist.ToLower()) 
                    //soundcloud doesnt have artist prop, check in title
                    : p.Title.ToLower().Contains(artist.ToLower()); 

                return isCorrectType 
                    && isCorrectTitle
                    && isCorrectArtist;
            }).ToList();
        }

        private bool IsCorrectType(string title, string songTitle, string type)
        {
            var isSupposedType = title.ToLower().Contains(type);
            var isType = songTitle.ToLower().Contains(type);
            return isSupposedType && isType || !isSupposedType && !isType;
        }

        private async Task<MeileSong> GetDetailsForMeileSong(string songId, HttpClient client)
        {
            var url = string.Format(MeileDetailUrl, songId);

            var resp = await client.GetAsync(url);
            var json = await resp.Content.ReadAsStringAsync();
            var parseResp = await json.DeserializeAsync<MeileDetailRoot>();

            if (parseResp == null || !resp.IsSuccessStatusCode) return null;

            return parseResp.values.songs.FirstOrDefault();
        }

        private async Task<NeteaseDetailSong> GetDetailsForNeteaseSong(int songId, HttpClient client)
        {
            var detailResp = await client.GetAsync(string.Format(NeteaseDetailApi, songId));
            var detailJson = await detailResp.Content.ReadAsStringAsync();
            var detailParseResp = await detailJson.DeserializeAsync<NeteaseDetailRoot>();
            if (!detailResp.IsSuccessStatusCode) throw new NetworkException();

            return detailParseResp.songs == null ? null : detailParseResp.songs.FirstOrDefault();
        }

        public async Task<List<WebSong>> SearchSoundCloud(string title, string artist, string album = null, int limit = 5)
        {
            var url = string.Format(SoundCloudSearchUrl, ApiKeys.SoundCloudId, limit, CreateQuery(title, artist, album));

            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(new Uri(url));

                var json = await resp.Content.ReadAsStringAsync();
                var parseResp = await json.DeserializeAsync<SoundCloudRoot>();

                if (parseResp == null || parseResp.collection == null || !resp.IsSuccessStatusCode) return null;

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

                return FilterByTypeAndMatch(parseResp.collection.Select(p => new WebSong(p)), 
                    title, artist);
            }
        }

        public async Task<List<WebSong>> SearchMp3Clan(string title, string artist, string album = null, int limit = 5)
        {
            var url = string.Format(Mp3ClanSearchUrl, CreateQuery(title, artist, album), limit);

            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(url);
                var json = await resp.Content.ReadAsStringAsync();
                var parseResp = await json.DeserializeAsync<Mp3ClanRoot>();

                if (parseResp == null || parseResp.response == null || !resp.IsSuccessStatusCode) return null;

                return FilterByTypeAndMatch(parseResp.response.Select(p => new WebSong(p)), 
                    title, artist);
            }
        }

        public async Task<List<WebSong>> SearchMeile(string title, string artist, string album = null, int limit = 5)
        {
            var url = string.Format(MeileSearchUrl, CreateQuery(title, artist, album));

            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(url);
                var html = await resp.Content.ReadAsStringAsync();

                //Meile has no search api, so we go to old school web page scrapping
                //using HtmlAgilityPack this is very easy
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                //Get the hyperlink node with the class='name'
                var songNameNodes = doc.DocumentNode.Descendants("a")
                    .Where(p => p.Attributes.Contains("class") && p.Attributes["class"].Value == "name");

                if (songNameNodes == null) return null;

                //in it there is an attribute that contains the url to the song
                var songUrls = songNameNodes.Select(p => p.Attributes["href"].Value);
                var songIds = songUrls.Where(p => p.Contains("/song/")).ToList();

                var songs = new List<WebSong>();

                foreach (var songId in songIds)
                {
                    var song = await GetDetailsForMeileSong(songId.Replace("/song/", ""), client);
                    if (song != null)
                    {
                        songs.Add(new WebSong(song));
                    }
                }

                return songs.Any() ? FilterByTypeAndMatch(songs, title, artist) : null;
            }
        }

        public async Task<List<WebSong>> SearchNetease(string title, string artist, string album = null, int limit = 5)
        {
            using (var client = new HttpClient())
            {
                //Setting referer (163 doesn't work without it)
                client.DefaultRequestHeaders.Referrer = new Uri("http://music.163.com");

                //Lets go ahead and search for a match
                var dict = new Dictionary<string, string>
                {
                    {"s", CreateQuery(title, artist, album, false)},
                    {"limit", limit.ToString()}
                };

                using (var data = new FormUrlEncodedContent(dict))
                {
                    var resp = await client.PostAsync(NeteaseSuggestApi, data);
                    var json = await resp.Content.ReadAsStringAsync();
                    var parseResp = await json.DeserializeAsync<NeteaseRoot>();
                    if (!resp.IsSuccessStatusCode) throw new NetworkException();

                    if (parseResp == null || parseResp.result == null || parseResp.result.songs == null) return null;

                    var songs = new List<WebSong>();

                    foreach (var neteaseSong in parseResp.result.songs)
                    {
                        var song = await GetDetailsForNeteaseSong(neteaseSong.id, client);
                        if (song != null)
                        {
                            songs.Add(new WebSong(song));
                        }
                    }

                    return songs.Any() ? FilterByTypeAndMatch(songs, title, artist) : null;
                }
            }
        }

        public Task<int> GetBitrateFromCc(string ccId)
        {
            throw new NotImplementedException();
        }
    }
}
