using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Audiotica.Core.Exceptions;
using Audiotica.Core.Utilities;
using Audiotica.Data.Model.LastFm;
using Audiotica.Data.Model.Musicbrainz;
using Audiotica.Data.Service.Interfaces;

namespace Audiotica.Data.Service.RunTime
{
    public class ScrobblerService : IScrobblerService
    {
        private const string BaseApiPath = 
            "http://ws.audioscrobbler.com/2.0/?method={0}&api_key={1}&format=json";
        private const string MbApiPath =
            "http://musicbrainz.org/ws/2/{0}/{1}?fmt=json";

        private void ThrowIfError(HttpResponseMessage resp)
        {
            if (!resp.IsSuccessStatusCode)
                throw new NetworkException();
        }

        private async Task<T> GetAsync<T>(string url)
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(url);
                ThrowIfError(resp);
                var json = await resp.Content.ReadAsStringAsync();
                var parseResp = await json.DeserializeAsync<T>();

                return parseResp;
            }
        }

        public async Task<MbRelease> GetMbAlbum(string id)
        {
            var url = string.Format(MbApiPath, "release", id);
            var resp = await GetAsync<MbRelease>(url);
            return resp;
        }

        public async Task<MbArtist> GetMbArtist(string id)
        {
            var url = string.Format(MbApiPath, "artist", id);
            var resp = await GetAsync<MbArtist>(url);
            return resp;
        }

        public async Task<FmDetailAlbum> GetDetailAlbum(string name, string artist)
        {
            var url = string.Format(BaseApiPath, "album.getInfo", ApiKeys.LastFmId);
            url += string.Format("&album={0}&artist={1}&autocorrect=1", name, artist);

            var resp = await GetAsync<FmDetailRoot>(url);
            return resp.album;
        }

        public async Task<FmDetailTrack> GetDetailTrack(string name, string artist)
        {
            var url = string.Format(BaseApiPath, "track.getInfo", ApiKeys.LastFmId);
            url += string.Format("&track={0}&artist={1}&autocorrect=1", name, artist);

            var resp = await GetAsync<FmDetailRoot>(url);
            return resp.track;
        }

        public async Task<FmDetailArtist> GetDetailArtist(string name)
        {
            var url = string.Format(BaseApiPath, "artist.getInfo", ApiKeys.LastFmId);
            url += string.Format("&artist={0}&autocorrect=1", name);

            var resp = await GetAsync<FmDetailRoot>(url);
            return resp.artist;
        }

        public async Task<FmResults> SearchTracksAsync(string query, int page = 1, int limit = 30)
        {
            var url = string.Format(BaseApiPath, "track.search", ApiKeys.LastFmId);
            url += string.Format("&track={0}&page={1}&limit={2}", query, page, limit);

            var resp = await GetAsync<FmSearchRoot>(url);

            return resp.results;
        }

        public async Task<FmResults> SearchArtistAsync(string query, int page = 1, int limit = 30)
        {
            var url = string.Format(BaseApiPath, "artist.search", ApiKeys.LastFmId);
            url += string.Format("&artist={0}&page={1}&limit={2}", query, page, limit);

            var resp = await GetAsync<FmSearchRoot>(url);
            return resp.results;
        }

        public async Task<FmResults> SearchAlbumsAsync(string query, int page = 1, int limit = 30)
        {
            var url = string.Format(BaseApiPath, "album.search", ApiKeys.LastFmId);
            url += string.Format("&album={0}&page={1}&limit={2}", query, page, limit);

            var resp = await GetAsync<FmSearchRoot>(url);
            return resp.results;
        }

        public async Task<FmTrackResults> GetTopTracksAsync(int page = 1, int limit = 30)
        {
            var url = string.Format(BaseApiPath, "chart.getTopTracks", ApiKeys.LastFmId);
            url += string.Format("&page={0}&limit={1}", page, limit);

            var resp = await GetAsync<FmChartsTopRoot>(url);
            return resp.tracks;
        }

        public async Task<FmArtistResults> GetTopArtistsAsync(int page = 1, int limit = 30)
        {
            var url = string.Format(BaseApiPath, "chart.getTopArtists", ApiKeys.LastFmId);
            url += string.Format("&page={0}&limit={1}", page, limit);

            var resp = await GetAsync<FmChartsTopRoot>(url);
            return resp.artists;
        }

        public async Task<FmArtistResults> GetSimilarArtistsAsync(int page = 1, int limit = 30)
        {
            var url = string.Format(BaseApiPath, "artist.getSimilar", ApiKeys.LastFmId);
            url += string.Format("&page={0}&limit={1}", page, limit);

            var resp = await GetAsync<FmChartsTopRoot>(url);
            return resp.artists;
        }

        public async Task<FmTrackResults> GetSimilarTracksAsync(int page = 1, int limit = 30)
        {
            var url = string.Format(BaseApiPath, "track.getSimilar", ApiKeys.LastFmId);
            url += string.Format("&page={0}&limit={1}", page, limit);

            var resp = await GetAsync<FmChartsTopRoot>(url);
            return resp.tracks;
        }
    }
}
