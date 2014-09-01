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
using System.Threading.Tasks;
using Audiotica.Core.Exceptions;
using Audiotica.Data.Service.Interfaces;
using Microsoft.Xbox.Music.Platform.Client;
using Microsoft.Xbox.Music.Platform.Contract.DataModel;

#endregion

namespace Audiotica.Data.Service.RunTime
{
    public class XboxMusicService : IXboxMusicService
    {
        private readonly IXboxMusicClient _client;

        public XboxMusicService()
        {
            //Create the xbox api client
            _client = XboxMusicClientFactory.CreateXboxMusicClient(ApiKeys.XboxClientId, ApiKeys.XboxClientSecret);
        }

        private void ThrowIfError(BaseResponse response)
        {
            if (response.Error != null)
                throw new XboxException(response.Error.Message, response.Error.Description);
        }

        public async Task<XboxPaginatedList<XboxArtist>> GetFeaturedArtist(int count = 10)
        {
            var results =
                await _client.BrowseAsync(Namespace.music, ContentSource.Catalog, ItemType.Artists, orderBy: OrderBy.MostPopular, maxItems:count);
            ThrowIfError(results);
            return results.Artists;
        }

        public async Task<XboxPaginatedList<XboxAlbum>> GetNewAlbums(int count = 10)
        {
            var results =
                await _client.BrowseAsync(Namespace.music, ContentSource.Catalog, ItemType.Albums, orderBy: OrderBy.ReleaseDate, maxItems: count);
            ThrowIfError(results);
            return results.Albums;
        }

        public async Task<XboxPaginatedList<XboxAlbum>> GetFeaturedAlbums(int count = 10)
        {
            var results =
               await _client.BrowseAsync(Namespace.music, ContentSource.Catalog, ItemType.Albums, orderBy: OrderBy.MostPopular, maxItems: count);
            ThrowIfError(results);
            return results.Albums;
        }
    }
}