#region

using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Audiotica.Core.Exceptions;
using Audiotica.Core.Utils;
using Audiotica.Data.Model.EchoNest;
using Audiotica.Data.Service.Interfaces;

#endregion

namespace Audiotica.Data.Service.RunTime
{
    public class EchoNestService : IEchoNestService
    {
        private const string EchoNestPath =
            "http://developer.echonest.com/api/v4/{0}/{1}?api_key=203JLLTVEDP9OTXP6&format=json";

        public async Task<EchoArtistUrls> GetArtistUrls(string name)
        {
            var url = string.Format(EchoNestPath, "artist", "urls") + "&name=" + name;
            var resp = await GetAsync<EchoRoot<EchoArtistUrlsRoot>>(url);
            ThrowIfError(resp);
            return resp.response.urls;
        }

        public async Task<EchoBiography> GetArtistBio(string name)
        {
            var url = string.Format(EchoNestPath, "artist", "biographies") + "&name=" + name;
            var resp = await GetAsync<EchoRoot<EchoBiographyRoot>>(url);
            ThrowIfError(resp);
            return resp.response.biographies.FirstOrDefault();
        }

        public async Task<EchoArtistVideosRoot> GetArtistVideos(string name, int start = 1, int limit = 15)
        {
            var url = string.Format(EchoNestPath, "artist", "video") + "&name={0}&start={1}&results={2}";
            url = string.Format(url, name, start, limit);

            var resp = await GetAsync<EchoRoot<EchoArtistVideosRoot>>(url);
            ThrowIfError(resp);
            return resp.response;
        }

        public async Task<EchoArtistImagesRoot> GetArtistImages(string name, int start = 1, int limit = 15)
        {
            var url = string.Format(EchoNestPath, "artist", "images") + "&name={0}&start={1}&results={2}";
            url = string.Format(url, name, start, limit);

            var resp = await GetAsync<EchoRoot<EchoArtistImagesRoot>>(url);
            ThrowIfError(resp);
            return resp.response;
        }

        private void ThrowIfError<T>(EchoRoot<T> echoResponse)
        {
            if (echoResponse == null || !(echoResponse.response is EchoResponse))
                throw new NetworkException();

            var resp = echoResponse.response as EchoResponse;
            if (resp.status.code != 0)
                throw new EchoException(resp.status.message);
        }

        private async Task<T> GetAsync<T>(string url)
        {
            using (var client = new HttpClient())
            {
                using (var resp = await client.GetAsync(url))
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var parseResp = await json.DeserializeAsync<T>();

                    return parseResp;
                }
            }
        }
    }
}