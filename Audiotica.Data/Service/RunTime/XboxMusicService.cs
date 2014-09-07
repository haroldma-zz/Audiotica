#region License

// Copyright (c) 2014 Harry Martinez <harry@zumicts.com>
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

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Audiotica.Core.Exceptions;
using Audiotica.Core.Utilities;
using Audiotica.Data.Model;
using Audiotica.Data.Service.Interfaces;
using Microsoft.Xbox.Music.Platform.Client;
using Microsoft.Xbox.Music.Platform.Contract.DataModel;

#endregion

namespace Audiotica.Data.Service.RunTime
{
    public class XboxMusicService : IXboxMusicService
    {
        private const string RelatedUrl =
            "https://eds.xboxlive.com/media/en-us/related?id={0}&desiredMediaItemTypes=MusicArtist&mediaItemType=MusicArtist&maxItems=10&fields=HkABAAAAAAAAAAAAAAAAAAYAEAY-&targetDevices=WindowsPC&firstPartyOnly=true";

        private readonly IXboxMusicClient _client;

        public XboxMusicService()
        {
            //Create the xbox api client
            _client = XboxMusicClientFactory.CreateXboxMusicClient(ApiKeys.XboxClientId, ApiKeys.XboxClientSecret);
        }

        public async Task<XboxPaginatedList<XboxArtist>> GetFeaturedArtist(int count = 10)
        {
            var results =
                await
                    _client.BrowseAsync(Namespace.music, ContentSource.Catalog, ItemType.Artists,
                        orderBy: OrderBy.MostPopular, maxItems: count);
            ThrowIfError(results);
            return results.Artists;
        }

        public async Task<XboxPaginatedList<XboxAlbum>> GetNewAlbums(int count = 10)
        {
            var results =
                await
                    _client.BrowseAsync(Namespace.music, ContentSource.Catalog, ItemType.Albums,
                        orderBy: OrderBy.ReleaseDate, maxItems: count);
            ThrowIfError(results);
            return results.Albums;
        }

        public async Task<XboxPaginatedList<XboxAlbum>> GetFeaturedAlbums(int count = 10)
        {
            var results =
                await
                    _client.BrowseAsync(Namespace.music, ContentSource.Catalog, ItemType.Albums,
                        orderBy: OrderBy.MostPopular, maxItems: count);
            ThrowIfError(results);
            return results.Albums;
        }

        public async Task<XboxAlbum> GetAlbumDetails(string id)
        {
            var results = await _client.LookupAsync(id, ContentSource.Catalog, extras: ExtraDetails.Tracks);
            ThrowIfError(results);
            return results.Albums.Items.FirstOrDefault();
        }

        public async Task<List<XboxArtist>> GetRelatedArtists(string id)
        {
            var results = await _client.LookupAsync(id, ContentSource.Catalog, extras: ExtraDetails.RelatedArtists);
            ThrowIfError(results);
            return results.Artists.Items;
        }

        #region undocumented api

        public async Task<List<XboxItem>> GetSpotlight()
        {
            using (var client = CreateHttpClient())
            {
                const string url = "http://mediadiscovery.xboxlive.com/en-us/music/feeds/2.0/spotlight";
                var resp = await client.GetAsync(url);
                var json = await resp.Content.ReadAsStringAsync();
                var parseResp = await json.DeserializeAsync<XboxFeedRoot>();
                return parseResp.Items;
            }
        }

        #endregion

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            });
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("XBLWIN/1.5.931");
            client.DefaultRequestHeaders.Add("x-xbl-build-version", @"current");
            client.DefaultRequestHeaders.Add("x-xbl-client-type", @"X13");
            client.DefaultRequestHeaders.Add("x-xbl-client-version", @"2.2.931.0");
            client.DefaultRequestHeaders.Add("x-xbl-contract-version", @"3.2");
            client.DefaultRequestHeaders.Add("x-xbl-device-type", @"Windows8_1PC");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.5");
            client.DefaultRequestHeaders.Add("UA-CPU", @"AMD64");

            return client;
        }

        private void ThrowIfError(BaseResponse response)
        {
            if (response.Error != null)
                throw new XboxException(response.Error.Message, response.Error.Description);
        }
    }
}