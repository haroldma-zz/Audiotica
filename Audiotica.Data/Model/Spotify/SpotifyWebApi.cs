﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Audiotica.Data.Model.Spotify.Models;
using Newtonsoft.Json;

namespace Audiotica.Data.Model.Spotify
{
    public class SpotifyWebApi : IDisposable
    {
        public String TokenType { get; set; }
        public String AccessToken { get; set; }

        readonly HttpClient _webclient;
        readonly JsonSerializerSettings _settings;
        public SpotifyWebApi()
        {
            _webclient = new HttpClient();
            _settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, TypeNameHandling = TypeNameHandling.All };
        }

        #region Search and Fetch
        public Task<SearchItem> SearchItems(String q, SearchType type, int limit = 20, int offset = 0)
        {
            limit = Math.Min(50, limit);
            var builder = new StringBuilder("https://api.spotify.com/v1/search");
            builder.Append("?q=" + q);
            builder.Append("&type=" + type.GetSearchValue(","));
            builder.Append("&limit=" + limit);
            builder.Append("&offset=" + offset);

            return DownloadDataAsync<SearchItem>(builder.ToString());
        }
        public Task<SeveralTracks> GetSeveralTracks(List<String> ids)
        {
            return DownloadDataAsync<SeveralTracks>("https://api.spotify.com/v1/tracks?ids=" + string.Join(",", ids));
        }
        public Task<SeveralAlbums> GetSeveralAlbums(List<String> ids)
        {
            return DownloadDataAsync<SeveralAlbums>("https://api.spotify.com/v1/albums?ids=" + string.Join(",", ids));
        }
        public Task<SeveralArtists> GetSeveralArtists(List<String> ids)
        {
            return DownloadDataAsync<SeveralArtists>("https://api.spotify.com/v1/artists?ids=" + string.Join(",", ids));
        }
        public Task<FullTrack> GetTrack(String id)
        {
            return DownloadDataAsync<FullTrack>("https://api.spotify.com/v1/tracks/" + id);
        }
        public Task<SeveralArtists> GetRelatedArtists(String id)
        {
            return DownloadDataAsync<SeveralArtists>("https://api.spotify.com/v1/artists/" + id + "/related-artists");
        }
        public Task<SeveralTracks> GetArtistsTopTracks(String id, String country)
        {
            return DownloadDataAsync<SeveralTracks>("https://api.spotify.com/v1/artists/" + id + "/top-tracks?country=" + country);
        }
        public Task<Paging<SimpleAlbum>> GetArtistsAlbums(String id, AlbumType type = AlbumType.ALL, String market = "", int limit = 20, int offset = 0)
        {
            limit = Math.Min(50, limit);
            var builder = new StringBuilder("https://api.spotify.com/v1/artists/" + id + "/albums");
            builder.Append("?type=" + type.GetAlbumValue(","));
            builder.Append("&limit=" + limit);
            builder.Append("&offset=" + offset);
            if (market != "")
                builder.Append("&market=" + market);
            return DownloadDataAsync<Paging<SimpleAlbum>>(builder.ToString());
        }
        public Task<FullArtist> GetArtist(String id)
        {
            return DownloadDataAsync<FullArtist>("https://api.spotify.com/v1/artists/" + id);
        }
        public Task<Paging<SimpleTrack>> GetAlbumTracks(String id, int limit = 20, int offset = 0)
        {
            limit = Math.Min(50, limit);
            var builder = new StringBuilder("https://api.spotify.com/v1/albums/" + id + "/tracks");
            builder.Append("?limit=" + limit);
            builder.Append("&offset=" + offset);
            return DownloadDataAsync<Paging<SimpleTrack>>(builder.ToString());
        }
        public Task<FullAlbum> GetAlbum(String id)
        {
            return DownloadDataAsync<FullAlbum>("https://api.spotify.com/v1/albums/" + id);
        }
        #endregion

        #region Util
        public async Task<T> DownloadDataAsync<T>(String url)
        {
            return JsonConvert.DeserializeObject<T>(await DownloadStringAsync(url), _settings);
        }

        public async Task<string> DownloadStringAsync(String url)
        {
            var resp = await _webclient.GetAsync(url);
            var text = await resp.Content.ReadAsStringAsync();
            Debug.WriteLine(text);
            return text;
        }

        #endregion

        public void Dispose()
        {
            _webclient.Dispose();
        }
    }
}