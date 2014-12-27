﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Audiotica.Core.Exceptions;
using Audiotica.Core.Utilities;
using Audiotica.Data.Model;
using Audiotica.Data.Model.SoundCloud;
using Audiotica.Data.Service.Interfaces;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
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
        private const string Mp3SkullSearchUrl = "http://mp3skull.com/search_db.php?q={0}";

        private const string Mp3TruckSearchUrl = "https://mp3truck.net/ajaxRequest.php";

        private const string MeileSearchUrl = "http://www.meile.com/search?type=&q={0}";
        private const string MeileDetailUrl = "http://www.meile.com/song/mult?songId={0}";

        private const string NeteaseSuggestApi = "http://music.163.com/api/search/suggest/web?csrf_token=";
        private const string NeteaseDetailApi = "http://music.163.com/api/song/detail/?ids=%5B{0}%5D";

        public async Task<List<WebSong>>  SearchYoutube(string title, string artist, string album = null, int limit = 5)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = ApiKeys.YoutubeId,
                ApplicationName = "Audiotica"
            });
            youtubeService.HttpClient.DefaultRequestHeaders.Referrer = new Uri("http://audiotica.fm");

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = CreateQuery(title, artist, album, false) + " (Audio)";
            searchListRequest.MaxResults = limit;

            try
            {
                // Call the search.list method to retrieve results matching the specified query term.
                var searchListResponse = await searchListRequest.ExecuteAsync();

                var songs = new List<WebSong>();

                foreach (var searchResult in searchListResponse.Items)
                {
                    if (searchResult.Id.Kind == "youtube#video")
                    {
                        var vid = new WebSong(searchResult);
                        using (var client = new HttpClient())
                        {
                            var resp = await client.GetAsync(
                                string.Format(
                                    "http://www.youtube-mp3.org/a/itemInfo/?video_id={0}&ac=www&t=grp&r=1419628947067&s=139194",
                                    vid.Id));
                            if (resp.IsSuccessStatusCode)
                            {
                                var json = await resp.Content.ReadAsStringAsync();
                                json = json.Replace(";", "").Replace("info = ", "").Replace("\"", "'");

                                var o = JToken.Parse(json);

                                if (o.Value<string>("status") == "serving")
                                {
                                    vid.Duration = TimeSpan.FromMinutes(o.Value<int>("length"));
                                    vid.AudioUrl =
                                        string.Format(
                                            "http://www.youtube-mp3.org/get?ab=128&video_id={0}&h={1}&r=1419629380530.1463092791&s=36098",
                                            vid.Id, o.Value<string>("h"));
                                    songs.Add(vid);
                                }
                            }
                        }
                    }
                }
                return FilterByTypeAndMatch(songs, title, artist);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<WebSong>> SearchSoundCloud(string title, string artist, string album = null,
            int limit = 10)
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

                return FilterByTypeAndMatch(parseResp.collection.Select(p => new WebSong(p)).ToList(),
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

                return FilterByTypeAndMatch(parseResp.response.Select(p => new WebSong(p)).ToList(),
                    title, artist);
            }
        }

        public async Task<List<WebSong>> SearchMp3Skull(string title, string artist, string album = null)        
        {
            var url = string.Format(Mp3SkullSearchUrl, CreateQuery(title, artist, album));

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

        public async Task<List<WebSong>> SearchMp3Truck(string title, string artist, string album = null)
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
                    var resp = await client.PostAsync(Mp3TruckSearchUrl, content);

                    if (!resp.IsSuccessStatusCode) return null;

                    var html = await resp.Content.ReadAsStringAsync();

                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    //Get the div node with the class='actl'
                    var songNodes = doc.DocumentNode.Descendants("div")
                        .Where(p => p.Attributes.Contains("class") && p.Attributes["class"].Value.Contains("actl"));

                    if (songNodes == null) return null;

                    var songs = new List<WebSong>();

                    foreach (var songNode in songNodes)
                    {
                        var song = new WebSong
                        {
                            Provider = Mp3Provider.Mp3Trunk
                        };

                        if (songNode.Attributes.Contains("data-id"))
                            song.Id = songNode.Attributes["data-id"].Value;
                        if (songNode.Attributes.Contains("data-bitrate"))
                            song.BitRate = int.Parse(songNode.Attributes["data-bitrate"].Value);
                        if (songNode.Attributes.Contains("data-filesize"))
                            song.ByteSize = (int) double.Parse(songNode.Attributes["data-filesize"].Value);
                        if (songNode.Attributes.Contains("data-duration"))
                        {
                            var duration = songNode.Attributes["data-duration"].Value;
                            var seconds = int.Parse(duration.Substring(duration.Length - 2, 2));
                            var minutes = int.Parse(duration.Replace(seconds.ToString(), ""));

                            song.Duration = new TimeSpan(0, 0, minutes, seconds);
                        }

                        var songTitle = songNode.Descendants("div")
                            .FirstOrDefault(p => p.Attributes.Contains("id")
                                                 && p.Attributes["id"].Value == "title").InnerText;
                        songTitle = WebUtility.HtmlDecode(songTitle.Substring(0, songTitle.Length - 4)).Trim();

                        //artist - title
                        var dashIndex = songTitle.IndexOf('-');
                        if (dashIndex != -1)
                        {
                            var titlePart = songTitle.Substring(dashIndex, songTitle.Length - dashIndex);
                            song.Artist = songTitle.Replace(titlePart, "").Trim();

                            songTitle = titlePart.Remove(0, 1).Trim();
                        }
                        song.Title = songTitle;

                        var linkNode = songNode.Descendants("a").FirstOrDefault(p => p.Attributes.Contains("class")
                                                                                     &&
                                                                                     p.Attributes["class"].Value
                                                                                         .Contains("mp3download"));
                        if (linkNode != null)
                        {
                            song.AudioUrl = linkNode.Attributes["href"]
                                .Value.Replace("/idl.php?u=", "");

                            songs.Add(song);
                        }
                    }

                    return songs.Any() ? FilterByTypeAndMatch(songs, title, artist) : null;
                }
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

        private string CreateQuery(string title, string artist, string album, bool urlEncode = true)
        {
            var query = ((title + " " + artist).Trim() + album).Trim();
            return urlEncode ? WebUtility.UrlEncode(query) : query;
        }

        private int GetMostUsedMinute(IEnumerable<WebSong> songs)
        {
            var minuteDic = new Dictionary<int, int>();
            var minuteList = songs.Where(p => p.Duration != null).Select(p => p.Duration.Minutes);

            foreach (var minute in minuteList)
            {
                if (minuteDic.ContainsKey(minute))
                {
                    minuteDic[minute]++;
                }
                else
                {
                    minuteDic.Add(minute, 1);
                }
            }

            return minuteDic.OrderByDescending(p => p.Value).FirstOrDefault().Key;
        }

        private List<WebSong> FilterByTypeAndMatch(IEnumerable<WebSong> songs, string title, string artist)
        {
            var filterSongs = songs.Where(p =>
            {
                var isCorrectType = IsCorrectType(title, p.Title, "mix")
                                    && IsCorrectType(title, p.Title, "cover")
                                    && IsCorrectType(title, p.Title, "live")
                                    && IsCorrectType(title, p.Title, "snipped")
                                    && IsCorrectType(title, p.Title, "preview")
                                    && IsCorrectType(title, p.Title, "acapella")
                                    && IsCorrectType(title, p.Title, "acoustic");

                var isCorrectTitle = p.Title.ToLower().Contains(title.ToLower())
                                     || title.ToLower().Contains(p.Title.ToLower());
                var isCorrectArtist = p.Artist != null
                    ? p.Artist.ToLower().Contains(artist.ToLower())
                      || artist.ToLower().Contains(p.Artist.ToLower()) 
                    //soundcloud doesnt have artist prop, check in title
                    : p.Title.ToLower().Contains(artist.ToLower());

                return isCorrectType
                       && isCorrectTitle
                       && isCorrectArtist;
            }).ToList();

            /*all the filter songs are candidates for being a match
             *but to improve it we can get how long do most songs last
             *those that don't meet it get removed.
             *Say, three songs last 3 minutes and two last 2 minutes,
             *it will be best to eliminate the two minute songs
             */
            var mostUsedMinute = GetMostUsedMinute(filterSongs);
            return filterSongs.Where(p => p.Duration.Minutes == mostUsedMinute).ToList();
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
    }
}