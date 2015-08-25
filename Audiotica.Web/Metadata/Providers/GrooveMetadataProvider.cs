using System;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Web.Enums;
using Audiotica.Web.Exceptions;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;
using Microsoft.Xbox.Music.Platform.Client;
using Microsoft.Xbox.Music.Platform.Contract.DataModel;

namespace Audiotica.Web.Metadata.Providers
{
    public class GrooveMetadataProvider : MetadataProviderWithSearchBase, IExtendedMetadataProvider, IDisposable
    {
        private readonly IXboxMusicClient _client;

        public GrooveMetadataProvider(ISettingsUtility settingsUtility) : base(settingsUtility)
        {
            _client = XboxMusicClientFactory.CreateXboxMusicClient(ApiKeys.XboxClientId, ApiKeys.XboxClientSecret);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public override string DisplayName => "Groove Music";
        public override ProviderSpeed Speed => ProviderSpeed.Average;
        public override ProviderCollectionSize CollectionSize => ProviderCollectionSize.Large;
        public override ProviderCollectionType CollectionQuality => ProviderCollectionType.GoodStuffMixedWithShit;

        public override async Task<WebAlbum> GetAlbumAsync(string albumToken)
        {
            var response = await _client.LookupAsync(albumToken, extras:ExtraDetails.Tracks);
            var xboxAlbum = response.Albums.Items.FirstOrDefault();

            if (response.Error == null)
                return CreateAlbum(xboxAlbum);

            // Something happened, throw exception
            throw new ProviderException(response.Error.Message);
        }

        public override async Task<WebSong> GetSongAsync(string songToken)
        {
            var response = await _client.LookupAsync(songToken);
            var xboxTrack = response.Tracks.Items.FirstOrDefault();

            if (response.Error == null)
                return CreateSong(xboxTrack);

            // Something happened, throw exception
            throw new ProviderException(response.Error.Message);
        }

        public override async Task<WebArtist> GetArtistAsync(string artistToken)
        {
            var response = await _client.LookupAsync(artistToken);
            var xboxArtist = response.Artists.Items.FirstOrDefault();

            if (response.Error == null)
                return CreateArtist(xboxArtist);

            // Something happened, throw exception
            throw new ProviderException(response.Error.Message);
        }

       
        public Task<WebResults> GetRelatedArtistsAsync(string artistToken, int limit = 50, string pageToken = null)
        {
            throw new NotImplementedException();
        }

        public async Task<WebResults> GetArtistTopSongsAsync(string artistToken, int limit = 50, string pageToken = null)
        {
            var response =
                await (pageToken == null
                    ? _client.SubBrowseAsync(artistToken, ContentSource.Catalog, BrowseItemType.Artist,
                        ExtraDetails.TopTracks, maxItems: limit)
                    : _client.SubBrowseContinuationAsync(artistToken, ContentSource.Catalog, BrowseItemType.Artist,
                        ExtraDetails.TopTracks, pageToken));

            var xboxArtist = response.Artists.Items.FirstOrDefault();

            if (response.Error == null)
            {
                var results = new WebResults
                {
                    HasMore = xboxArtist.TopTracks?.ContinuationToken != null,
                    PageToken = xboxArtist.TopTracks?.ContinuationToken,
                    Songs = xboxArtist.TopTracks?.Items?.Select(CreateSong).ToList()
                };
                return results;
            }

            throw new ProviderException(response.Error.Message);
        }

        public async Task<WebResults> GetArtistAlbumsAsync(string artistToken, int limit = 50, string pageToken = null)
        {
            var response =
                await (pageToken == null
                    ? _client.SubBrowseAsync(artistToken, ContentSource.Catalog, BrowseItemType.Artist,
                        ExtraDetails.Albums, OrderBy.MostPopular, limit)
                    : _client.SubBrowseContinuationAsync(artistToken, ContentSource.Catalog, BrowseItemType.Artist,
                        ExtraDetails.Albums, pageToken));
            var xboxArtist = response.Artists.Items.FirstOrDefault();

            if (response.Error == null)
            {
                var results = new WebResults
                {
                    HasMore = xboxArtist.Albums?.ContinuationToken != null,
                    PageToken = xboxArtist.Albums?.ContinuationToken,
                    Albums = xboxArtist.Albums?.Items?.Select(CreateAlbum).Distinct(new WebAlbum.Comparer()).ToList()
                };
                return results;
            }

            throw new ProviderException(response.Error.Message);
        }

        public async Task<WebResults> GetArtistNewAlbumsAsync(string artistToken, int limit = 50,
           string pageToken = null)
        {
            var response =
                await (pageToken == null
                    ? _client.SubBrowseAsync(artistToken, ContentSource.Catalog, BrowseItemType.Artist,
                        ExtraDetails.Albums, OrderBy.ReleaseDate, limit)
                    : _client.SubBrowseContinuationAsync(artistToken, ContentSource.Catalog, BrowseItemType.Artist,
                        ExtraDetails.Albums, pageToken));
            var xboxArtist = response.Artists.Items.FirstOrDefault();

            if (response.Error == null)
            {
                var results = new WebResults
                {
                    HasMore = xboxArtist.Albums?.ContinuationToken != null,
                    PageToken = xboxArtist.Albums?.ContinuationToken,
                    Albums = xboxArtist.Albums?.Items?.Select(CreateAlbum).Distinct(new WebAlbum.Comparer()).ToList()
                };
                return results;
            }

            throw new ProviderException(response.Error.Message);
        }

        public override async Task<WebResults> SearchAsync(string query,
            WebResults.Type searchType = WebResults.Type.Song,
            int limit = 20, string pageToken = null)
        {
            switch (searchType)
            {
                default:
                    var songResponse = await
                        (pageToken == null
                            ? _client.SearchAsync(Namespace.music, query, ContentSource.Catalog, SearchFilter.Tracks,
                                maxItems: limit)
                            : _client.SearchContinuationAsync(Namespace.music, pageToken));
                    var songResults = new WebResults
                    {
                        HasMore = songResponse.Tracks?.ContinuationToken != null,
                        PageToken = songResponse.Tracks?.ContinuationToken,
                        Songs = songResponse.Tracks?.Items?.Select(CreateSong).ToList()
                    };
                    return songResults;
                case WebResults.Type.Artist:
                    var artistResponse =
                        await
                            (pageToken == null
                                ? _client.SearchAsync(Namespace.music, query, ContentSource.Catalog,
                                    SearchFilter.Artists, maxItems: limit)
                                : _client.SearchContinuationAsync(Namespace.music, pageToken));
                    var artistResults = new WebResults
                    {
                        HasMore = artistResponse.Artists?.ContinuationToken != null,
                        PageToken = artistResponse.Artists?.ContinuationToken,
                        Artists = artistResponse.Artists?.Items?.Select(CreateArtist).ToList()
                    };
                    return artistResults;
                case WebResults.Type.Album:
                    var albumResponse =
                        await
                            (pageToken == null
                                ? _client.SearchAsync(Namespace.music, query, ContentSource.Catalog, SearchFilter.Albums,
                                    maxItems: limit)
                                : _client.SearchContinuationAsync(Namespace.music, pageToken));
                    var albumResults = new WebResults
                    {
                        HasMore = albumResponse.Albums?.ContinuationToken != null,
                        PageToken = albumResponse.Albums?.ContinuationToken,
                        Albums =
                            albumResponse.Albums?.Items?.Select(CreateAlbum).Distinct(new WebAlbum.Comparer()).ToList()
                    };
                    return albumResults;
            }
        }

        #region Helpers

        private WebArtist CreateArtist(XboxArtist xboxArtist)
        {
            var artist = new WebArtist(GetType())
            {
                Name = xboxArtist.Name,
                Token = xboxArtist.Id
            };

            if (xboxArtist.ImageUrl == null)
                artist.IsPartial = true;
            else
                artist.Artwork = new Uri(xboxArtist.ImageUrl);

            return artist;
        }

        public WebAlbum CreateAlbum(XboxAlbum xboxAlbum)
        {
            var album = new WebAlbum(GetType())
            {
                Title = xboxAlbum.Name,
                Token = xboxAlbum.Id,
                Genres = xboxAlbum.Genres?.ToList(),
                ReleaseDate = xboxAlbum.ReleaseDate
            };

            if (xboxAlbum.ImageUrl == null)
                album.IsPartial = true;
            else
                album.Artwork = new Uri(xboxAlbum.GetImageUrl(500, 500));


            if (xboxAlbum.Artists == null)
                album.IsPartial = true;
            else
                album.Artist = CreateArtist(xboxAlbum.PrimaryArtist);

            if (xboxAlbum.ImageUrl == null)
                album.IsPartial = true;
            else
                album.Artwork = new Uri(xboxAlbum.ImageUrl);

            if (xboxAlbum.Tracks == null)
                album.IsPartial = true;
            else
            {
                // TODO make sure to get all (continuation token)
                album.Tracks = xboxAlbum.Tracks?.Items?.Select(CreateSong).ToList();
            }

            return album;
        }

        private WebSong CreateSong(XboxTrack xboxTrack)
        {
            var song = new WebSong(GetType())
            {
                Title = xboxTrack.Name + (string.IsNullOrEmpty(xboxTrack.Subtitle) ? "" : $" ({xboxTrack.Subtitle})"),
                Token = xboxTrack.Id,
                TrackNumber = xboxTrack.TrackNumber ?? 1,
                DiskNumber = 1
            };

            if (xboxTrack.Artists != null)
                song.Artists = xboxTrack.Artists.Select(p => CreateArtist(p.Artist)).ToList();
            else
                song.IsPartial = true;

            if (xboxTrack.Album != null)
                song.Album = CreateAlbum(xboxTrack.Album);
            else
                song.IsPartial = true;

            return song;
        }

        #endregion
    }
}