#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Audiotica.Core.Utils;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Data.Model;
using Audiotica.Data.Model.SoundCloud;
using Audiotica.Data.Service.Interfaces;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

#endregion

namespace Audiotica.Data.Service.RunTime
{
    public class Mp3SearchService : IMp3SearchService
    {
        private const string SoundCloudSearchUrl =
            "https://api.soundcloud.com/search/sounds?client_id={0}&limit={1}&q={2}";

        private const string Mp3ClanSearchUrl = "http://mp3clan.com/app/mp3Search.php?q={0}&count={1}";

        private const string PleerSearchUrl =
            "http://pleer.com/search?page=1&q={0}&sort_mode=0&sort_by=0&quality=best&onlydata=true";

        private const string PleerFileUrl = "http://pleer.com/site_api/files/get_url";
        private const string Mp3TruckSearchUrl = "https://mp3truck.net/ajaxRequest.php";
        private const string MeileSearchUrl = "http://www.meile.com/search?type=&q={0}";
        private const string MeileDetailUrl = "http://www.meile.com/song/mult?songId={0}";
        private const string NeteaseSuggestApi = "http://music.163.com/api/search/suggest/web?csrf_token=";
        private const string NeteaseDetailApi = "http://music.163.com/api/song/detail/?ids=%5B{0}%5D";
        private const string Mp3SkullSearchUrl = "http://mp3skull.com/search_db.php?q={0}&fckh={1}";
        private readonly IAppSettingsHelper _appSettingsHelper;
        private string _mp3SkullFckh;

        public Mp3SearchService(IAppSettingsHelper appSettingsHelper)
        {
            this._appSettingsHelper = appSettingsHelper;
            HtmlNode.ElementsFlags.Remove("form");
        }

        private string Mp3SkullFckh
        {
            get { return _mp3SkullFckh ?? (_mp3SkullFckh = _appSettingsHelper.Read<string>("Mp3SkullFckh")); }

            set
            {
                _mp3SkullFckh = value;
                _appSettingsHelper.Write("Mp3SkullFckh", value);
            }
        }

        public async Task<List<WebSong>> SearchSoundCloud(
            string title,
            string artist,
            string album = null,
            int limit = 10, bool checkAllLinks = false)
        {
            var url = string.Format(
                SoundCloudSearchUrl,
                ApiKeys.SoundCloudId,
                limit,
                CreateQuery(title, artist, album));

            using (var client = new HttpClient())
            {
                using (var resp = await client.GetAsync(url))
                {
                    var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var parseResp = await json.DeserializeAsync<SoundCloudRoot>().ConfigureAwait(false);

                    if (parseResp == null || parseResp.collection == null || !resp.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    // Remove those that can't be stream
                    parseResp.collection.RemoveAll(p => p.stream_url == null);

                    foreach (var song in parseResp.collection)
                    {
                        if (song.stream_url.Contains("soundcloud") && !song.stream_url.Contains("client_id"))
                        {
                            song.stream_url += "?client_id=" + ApiKeys.SoundCloudId;
                        }

                        if (string.IsNullOrEmpty(song.artwork_url))
                        {
                            song.artwork_url = song.user.avatar_url;
                        }

                        if (song.artwork_url.IndexOf('?') > -1)
                        {
                            song.artwork_url = song.artwork_url.Remove(song.artwork_url.LastIndexOf('?'));
                        }

                        song.artwork_url = song.artwork_url.Replace("large", "t500x500");
                    }

                    return
                        await
                            IdentifyMatches(parseResp.collection.Select(p => new WebSong(p)).ToList(), title,
                                artist, checkAllLinks);
                }
            }
        }

        public async Task<List<WebSong>> SearchMp3Clan(string title, string artist, string album = null, int limit = 10,
            bool checkAllLinks = false)
        {
            // mp3clan search doesn't work that well with the pound key (even encoded)
            var url = string.Format(
                Mp3ClanSearchUrl,
                CreateQuery(title.Contains("#") ? title.Replace("#", string.Empty) : title, artist, album),
                limit);

            using (var client = new HttpClient())
            {
                using (var resp = await client.GetAsync(url).ConfigureAwait(false))
                {
                    var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var parseResp = await json.DeserializeAsync<Mp3ClanRoot>().ConfigureAwait(false);

                    if (parseResp == null || parseResp.response == null || !resp.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    return
                        await
                            IdentifyMatches(parseResp.response.Select(p => new WebSong(p)).ToList(), title, artist,
                                checkAllLinks);
                }
            }
        }

        public async Task<List<WebSong>> SearchMp3Truck(string title, string artist, string album = null,
            bool checkAllLinks = false)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Referrer = new Uri("https://mp3truck.net/");
                var data = new Dictionary<string, string>
                {
                    {"sort", "relevance"},
                    {"p", "1"},
                    {"q", CreateQuery(title, artist, album)}
                };

                using (var content = new FormUrlEncodedContent(data))
                {
                    using (var resp = await client.PostAsync(Mp3TruckSearchUrl, content).ConfigureAwait(false))
                    {
                        if (!resp.IsSuccessStatusCode)
                        {
                            return null;
                        }

                        var html = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

                        var doc = new HtmlDocument();
                        doc.LoadHtml(html);

                        // Get the div node with the class='actl'
                        var songNodes =
                            doc.DocumentNode.Descendants("div")
                                .Where(
                                    p => p.Attributes.Contains("class") && p.Attributes["class"].Value.Contains("actl"));

                        var songs = new List<WebSong>();

                        foreach (var songNode in songNodes)
                        {
                            var song = new WebSong {Provider = Mp3Provider.Mp3Truck};

                            if (songNode.Attributes.Contains("data-id"))
                            {
                                song.Id = songNode.Attributes["data-id"].Value;
                            }

                            if (songNode.Attributes.Contains("data-bitrate"))
                            {
                                song.BitRate = int.Parse(songNode.Attributes["data-bitrate"].Value);
                            }

                            if (songNode.Attributes.Contains("data-filesize"))
                            {
                                song.ByteSize = (int) double.Parse(songNode.Attributes["data-filesize"].Value);
                            }

                            if (songNode.Attributes.Contains("data-duration"))
                            {
                                var duration = songNode.Attributes["data-duration"].Value;

                                if (duration.Contains(":"))
                                {
                                    var seconds = int.Parse(duration.Substring(duration.Length - 2, 2));
                                    var minutes = int.Parse(duration.Remove(duration.Length - 3));
                                    song.Duration = new TimeSpan(0, 0, minutes, seconds);
                                }
                                else
                                {
                                    song.Duration = new TimeSpan(0, 0, 0, int.Parse(duration));
                                }
                            }

                            var songTitle =
                                songNode.Descendants("div")
                                    .FirstOrDefault(
                                        p => p.Attributes.Contains("id") && p.Attributes["id"].Value == "title")
                                    .InnerText;
                            songTitle = WebUtility.HtmlDecode(songTitle.Substring(0, songTitle.Length - 4)).Trim();

                            // artist - title
                            var dashIndex = songTitle.IndexOf('-');
                            if (dashIndex != -1)
                            {
                                var titlePart = songTitle.Substring(dashIndex, songTitle.Length - dashIndex);
                                song.Artist = songTitle.Replace(titlePart, string.Empty).Trim();

                                songTitle = titlePart.Remove(0, 1).Trim();
                            }

                            song.Name = songTitle;

                            var linkNode =
                                songNode.Descendants("a")
                                    .FirstOrDefault(
                                        p =>
                                            p.Attributes.Contains("class")
                                            && p.Attributes["class"].Value.Contains("mp3download"));
                            if (linkNode == null)
                            {
                                continue;
                            }

                            song.AudioUrl = linkNode.Attributes["href"].Value.Replace("/idl.php?u=", string.Empty);

                            songs.Add(song);
                        }

                        return songs.Any() ? await IdentifyMatches(songs, title, artist, checkAllLinks) : null;
                    }
                }
            }
        }

        public async Task<List<WebSong>> SearchMeile(string title, string artist, string album = null, int limit = 10,
            bool checkAllLinks = false)
        {
            var url = string.Format(MeileSearchUrl, CreateQuery(title, artist, album));

            using (var client = new HttpClient())
            {
                using (var resp = await client.GetAsync(url).ConfigureAwait(false))
                {
                    var html = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

                    // Meile has no search api, so we go to old school web page scrapping
                    // using HtmlAgilityPack this is very easy
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    // Get the hyperlink node with the class='name'
                    var songNameNodes =
                        doc.DocumentNode.Descendants("a")
                            .Where(p => p.Attributes.Contains("class") && p.Attributes["class"].Value == "name");

                    // in it there is an attribute that contains the url to the song
                    var songUrls = songNameNodes.Select(p => p.Attributes["href"].Value);
                    var songIds = songUrls.Where(p => p.Contains("/song/")).ToList();

                    var songs = new List<WebSong>();

                    foreach (var songId in songIds)
                    {
                        var song =
                            await
                                GetDetailsForMeileSong(songId.Replace("/song/", string.Empty), client)
                                    .ConfigureAwait(false);
                        if (song != null)
                        {
                            songs.Add(new WebSong(song));
                        }
                    }

                    return songs.Any() ? await IdentifyMatches(songs, title, artist, checkAllLinks) : null;
                }
            }
        }

        public async Task<List<WebSong>> SearchNetease(string title, string artist, string album = null, int limit = 10,
            bool checkAllLinks = false)
        {
            using (var client = new HttpClient())
            {
                // Setting referer (163 doesn't work without it)
                client.DefaultRequestHeaders.Referrer = new Uri("http://music.163.com");

                // Lets go ahead and search for a match
                var dict = new Dictionary<string, string>
                {
                    {"s", CreateQuery(title, artist, album, false)},
                    {"limit", limit.ToString()}
                };

                using (var data = new FormUrlEncodedContent(dict))
                {
                    using (var resp = await client.PostAsync(NeteaseSuggestApi, data).ConfigureAwait(false))
                    {
                        var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var parseResp = await json.DeserializeAsync<NeteaseRoot>();
                        if (!resp.IsSuccessStatusCode)
                        {
                            return null;
                        }

                        if (parseResp == null || parseResp.result == null || parseResp.result.songs == null)
                        {
                            return null;
                        }

                        var songs = new List<WebSong>();

                        foreach (var neteaseSong in parseResp.result.songs)
                        {
                            var song = await GetDetailsForNeteaseSong(neteaseSong.id, client).ConfigureAwait(false);
                            if (song != null)
                            {
                                songs.Add(new WebSong(song));
                            }
                        }

                        return songs.Any() ? await IdentifyMatches(songs, title, artist, checkAllLinks) : null;
                    }
                }
            }
        }

        public Task<int> GetBitrateFromCc(string ccId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<WebSong>> SearchPleer(string title, string artist, string album = null,
            bool checkAllLinks = false)
        {
            var url = string.Format(
                PleerSearchUrl,
                CreateQuery(title, artist, album));

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                using (var resp = await client.GetAsync(url))
                {
                    if (!resp.IsSuccessStatusCode) return null;

                    var json = await resp.Content.ReadAsStringAsync();
                    var o = JToken.Parse(json);

                    if (!o.Value<bool>("success")) return null;

                    var html = o.Value<string>("html");

                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var songNodes = doc.DocumentNode.Descendants("li");

                    var songs = new List<WebSong>();

                    foreach (var songNode in songNodes)
                    {
                        var song = new WebSong();

                        song.Id = songNode.Attributes["file_id"].Value;
                        song.Name = songNode.Attributes["song"].Value;
                        song.Artist = songNode.Attributes["singer"].Value;

                        int bitRate;
                        if (int.TryParse(songNode.Attributes["rate"].Value.Replace(" Kb/s", ""), out bitRate))
                        {
                            song.BitRate = bitRate;
                        }
                        int seconds;
                        if (int.TryParse(songNode.Attributes["duration"].Value, out seconds))
                        {
                            song.Duration = TimeSpan.FromSeconds(seconds);
                        }

                        var linkId = songNode.Attributes["link"].Value;
                        song.AudioUrl = await GetPleerLinkAsync(client, linkId);

                        if (string.IsNullOrEmpty(song.AudioUrl)) continue;

                        songs.Add(song);
                    }

                    return await IdentifyMatches(songs, title, artist, checkAllLinks);
                }
            }
        }

        private async Task<string> GetPleerLinkAsync(HttpClient client, string id)
        {
            var data = new Dictionary<string, string>
            {
                {"action", "download"},
                {"id", id}
            };

            using (var content = new FormUrlEncodedContent(data))
            {
                using (var resp = await client.PostAsync(PleerFileUrl, content))
                {
                    if (!resp.IsSuccessStatusCode) return null;

                    var json = await resp.Content.ReadAsStringAsync();
                    var o = JToken.Parse(json);
                    return o.Value<string>("track_link");
                }
            }
        }

        public async Task<List<WebSong>> SearchMp3Skull(string title, string artist, string album = null,
            bool checkAllLinks = false)
        {
            using (var client = new HttpClient())
            {
                var url = string.Format(Mp3SkullSearchUrl, CreateQuery(title, artist, album), Mp3SkullFckh);
                using (var resp = await client.GetAsync(url).ConfigureAwait(false))
                {
                    if (!resp.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    var html = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    if (html.Contains("You have made too many request"))
                    {
                        return null;
                    }

                    if (html.Contains("Your search session has expired"))
                    {
                        var fckhNode =
                            doc.DocumentNode.Descendants("input")
                                .FirstOrDefault(
                                    p => p.Attributes.Contains("name") && p.Attributes["name"].Value == "fckh");
                        if (fckhNode == null)
                        {
                            return null;
                        }

                        Mp3SkullFckh = fckhNode.Attributes["value"].Value;

                        return await SearchMp3Skull(title, artist, album);
                    }

                    // Get the div node
                    var songNodes = doc.DocumentNode.Descendants("div").Where(p => p.Id == "song_html");

                    var songs = new List<WebSong>();

                    foreach (var songNode in songNodes)
                    {
                        var song = new WebSong {Provider = Mp3Provider.Mp3Skull};

                        var songUrlNode = songNode.Descendants("a").FirstOrDefault(p => p.InnerText == "Download");

                        if (songUrlNode == null)
                        {
                            continue;
                        }

                        song.AudioUrl = songUrlNode.Attributes["href"].Value;
                        var songInfo =
                            songNode.Descendants("div")
                                .FirstOrDefault(p => p.Attributes["class"].Value == "left")
                                .InnerText.Replace("<!-- info mp3 here -->", string.Empty)
                                .Trim();


                        var bitRateIndex = songInfo.IndexOf("kbps", StringComparison.Ordinal);
                        if (bitRateIndex > -1)
                        {
                            var bitrateTxt = songInfo.Substring(0, bitRateIndex);
                            int bitrate;
                            if (int.TryParse(bitrateTxt, out bitrate))
                            {
                                song.BitRate = bitrate;
                            }
                        }

                        #region Duration

                        if (bitRateIndex > -1)
                        {
                            songInfo = songInfo.Remove(0, bitRateIndex + 4);
                        }

                        var durationIndex = songInfo.IndexOf(":", StringComparison.Ordinal);
                        if (durationIndex > -1)
                        {
                            var durationText = songInfo.Substring(0, durationIndex + 3);
                            var seconds = int.Parse(durationText.Substring(durationText.Length - 2, 2));
                            var minutes = int.Parse(durationText.Remove(durationText.Length - 3));

                            song.Duration = new TimeSpan(0, 0, minutes, seconds);
                        }

                        #endregion

                        #region Size

                        if (durationIndex > -1)
                        {
                            songInfo = songInfo.Remove(0, durationIndex + 3);
                        }

                        var sizeIndex = songInfo.IndexOf("mb", StringComparison.Ordinal);
                        if (sizeIndex > -1)
                        {
                            var sizeText = songInfo.Substring(0, sizeIndex);
                            long size;
                            if (long.TryParse(sizeText, out size))
                            {
                                song.ByteSize = (long) (size*(1024*1024.0));
                            }
                        }

                        #endregion

                        var songTitle = songNode.Descendants("b").FirstOrDefault().InnerText;
                        songTitle = songTitle.Substring(0, songTitle.Length - 4).Trim();

                        song.Name = songTitle;
                        
                        songs.Add(song);
                    }

                    return songs.Any() ? await IdentifyMatches(songs, title, artist, checkAllLinks) : null;
                }
            }
        }

        private string CreateQuery(string title, string artist, string album, bool urlEncode = true)
        {
            var query = ((title + " " + artist).Trim() + album).Trim();
            return urlEncode ? WebUtility.UrlEncode(query) : query;
        }

        private async Task<List<WebSong>> IdentifyMatches(List<WebSong> songs, string title, string artist,
            bool checkAll)
        {
            songs = songs.OrderBy(p => p.Duration.Minutes).ToList();

            foreach (var webSong in songs)
            {
                webSong.Name = webSong.Name.ToLower().Replace("feat.", "ft.").ToCleanQuery();
                webSong.Artist = webSong.Artist.ToCleanQuery();

                if (string.IsNullOrEmpty(webSong.Name)) continue;

                var isCorrectType = IsCorrectType(title, webSong.Name, "mix")
                                    && IsCorrectType(title, webSong.Name, "cover")
                                    && IsCorrectType(title, webSong.Name, "rmx")
                                    && IsCorrectType(title, webSong.Name, "live")
                                    && IsCorrectType(title, webSong.Name, "snipped")
                                    && IsCorrectType(title, webSong.Name, "preview")
                                    && IsCorrectType(title, webSong.Name, "acapella")
                                    && IsCorrectType(title, webSong.Name, "radio")
                                    && IsCorrectType(title, webSong.Name, "acoustic");

                var isCorrectTitle = webSong.Name.Contains(title)
                                     || title.Contains(webSong.Name);
                var isCorrectArtist = webSong.Artist != null
                    ? webSong.Artist.Contains(artist)
                      || artist.Contains(webSong.Artist)

                    // soundcloud doesnt have artist prop, check in title
                    : webSong.Name.Contains(artist);

                webSong.IsMatch = isCorrectType && isCorrectTitle && isCorrectArtist;
            }

            var filterSongs = songs.Where(p => p.IsMatch).ToList();

            /*all the filter songs are candidates for being a match
             *but to improve it we can get how long do most songs last
             *those that don't meet it get removed.
             *Say, three songs last 3 minutes and two last 2 minutes,
             *it will be best to eliminate the two minute songs
             */
            var mostUsedMinute = GetMostUsedMinute(filterSongs);

            foreach (var webSong in checkAll ? songs : filterSongs)
            {
                if (await IsUrlOnlineAsync(webSong))
                    webSong.IsBestMatch = webSong.IsMatch && webSong.Duration.Minutes == mostUsedMinute;
                else
                    webSong.IsLinkDeath = true;
            }

            return songs.OrderByDescending(p => p.ByteSize).ToList();
        }

        private async Task<MeileSong> GetDetailsForMeileSong(string songId, HttpClient client)
        {
            var url = string.Format(MeileDetailUrl, songId);

            using (var resp = await client.GetAsync(url))
            {
                var json = await resp.Content.ReadAsStringAsync();

                var parseResp = await json.DeserializeAsync<MeileDetailRoot>();

                if (parseResp == null || !resp.IsSuccessStatusCode)
                {
                    return null;
                }

                return parseResp.values.songs.FirstOrDefault();
            }
        }

        private async Task<NeteaseDetailSong> GetDetailsForNeteaseSong(int songId, HttpClient client)
        {
            using (var detailResp = await client.GetAsync(string.Format(NeteaseDetailApi, songId)))
            {
                var detailJson = await detailResp.Content.ReadAsStringAsync();
                var detailParseResp = await detailJson.DeserializeAsync<NeteaseDetailRoot>();
                if (!detailResp.IsSuccessStatusCode || detailParseResp == null)
                {
                    return null;
                }

                return detailParseResp.songs == null ? null : detailParseResp.songs.FirstOrDefault();
            }
        }

        private int GetMostUsedMinute(List<WebSong> songs)
        {
            var minuteDic = new Dictionary<int, int>();
            var minuteList = songs.Select(p => p.Duration.Minutes);

            foreach (var minute in minuteList)
            {
                int value;
                if (minuteDic.TryGetValue(minute, out value))
                {
                    minuteDic[minute] = ++value;
                }
                else
                {
                    minuteDic.Add(minute, 1);
                }
            }

            if (minuteDic.ContainsKey(0) && minuteDic.Count > 1)
            {
                minuteDic.Remove(0);
            }

            return minuteDic.OrderByDescending(p => p.Value).FirstOrDefault().Key;
        }

        private bool IsCorrectType(string title, string songTitle, string type)
        {
            var isSupposedType = title.Contains(" " + type) || title.Contains(type + " ") || title.Contains(type + ")")
                                 || title.Contains("(" + type);
            var isType = songTitle.Contains(" " + type) || songTitle.Contains(type + " ")
                         || songTitle.Contains(type + ")") || songTitle.Contains("(" + type);
            return (isSupposedType && isType) || (!isSupposedType && !isType);
        }

        private async Task<bool> IsUrlOnlineAsync(WebSong song)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    using (var resp =
                        await
                            client.SendAsync(
                                new HttpRequestMessage(HttpMethod.Head, new Uri(song.AudioUrl)),
                                HttpCompletionOption.ResponseHeadersRead))
                    {
                        if (!resp.IsSuccessStatusCode) return false;

                        if (!resp.Content.Headers.ContentType.MediaType.Contains("audio")
                            && !resp.Content.Headers.ContentType.MediaType.Contains("octet-stream"))
                        {
                            return false;
                        }

                        var size = resp.Content.Headers.ContentLength;
                        if (size != null)
                        {
                            song.ByteSize = (long) size;
                        }
                        return song.ByteSize > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}