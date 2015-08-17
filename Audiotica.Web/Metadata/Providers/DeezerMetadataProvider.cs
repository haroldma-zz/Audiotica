using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Web.Enums;
using Audiotica.Web.Exceptions;
using Audiotica.Web.Http.Requets.Metadata.Deezer;
using Audiotica.Web.Http.Requets.Metadata.Deezer.Models;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;
using Newtonsoft.Json.Linq;

namespace Audiotica.Web.Metadata.Providers
{
    public class DeezerMetadataProvider : MetadataProviderWithSearchBase, IExtendedMetadataProvider,
        IChartMetadataProvider
    {
        public DeezerMetadataProvider(ISettingsUtility settingsUtility) : base(settingsUtility)
        {
        }

        public async Task<WebResults> GetTopSongsAsync(int limit = 20, string pageToken = null)
        {
            int offset;
            int.TryParse(pageToken, out offset);

            using (
                var response =
                    await new DeezerChartRequest<JToken>(WebResults.Type.Song)
                        .Limit(limit)
                        .Offset(offset)
                        .ToResponseAsync()
                        .DontMarshall())
            {
                if (!response.HasData) return null;

                var results = CreateResults(response.Data, limit, offset);
                results.Songs = response.Data.Data?.Select(CreateSong).ToList();

                return results;
            }
        }

        public async Task<WebResults> GetTopAlbumsAsync(int limit = 20, string pageToken = null)
        {
            int offset;
            int.TryParse(pageToken, out offset);

            using (
                var response =
                    await new DeezerChartRequest<JToken>(WebResults.Type.Album)
                        .Limit(limit)
                        .Offset(offset)
                        .ToResponseAsync()
                        .DontMarshall())
            {
                if (!response.HasData) return null;

                var results = CreateResults(response.Data, limit, offset);
                results.Albums = response.Data.Data?.Select(CreateAlbum).ToList();

                return results;
            }
        }

        public async Task<WebResults> GetTopArtistsAsync(int limit = 20, string pageToken = null)
        {
            int offset;
            int.TryParse(pageToken, out offset);

            using (
                var response =
                    await new DeezerChartRequest<JToken>(WebResults.Type.Artist)
                        .Limit(limit)
                        .Offset(offset)
                        .ToResponseAsync()
                        .DontMarshall())
            {
                if (!response.HasData) return null;

                var results = CreateResults(response.Data, limit, offset);
                results.Artists = response.Data.Data?.Select(CreateArtist).ToList();

                return results;
            }
        }

        public override string DisplayName => "Deezer";
        public override ProviderSpeed Speed => ProviderSpeed.Fast;
        public override ProviderCollectionSize CollectionSize => ProviderCollectionSize.Large;
        public override ProviderCollectionType CollectionType => ProviderCollectionType.MainstreamAndRare;

        public async Task<WebResults> GetArtistTopSongsAsync(string artistToken, int limit = 20,
            string pageToken = null)
        {
            int offset;
            int.TryParse(pageToken, out offset);

            using (
                var response =
                    await new DeezerArtistTopTracksRequest(int.Parse(artistToken))
                        .Limit(limit)
                        .Offset(offset)
                        .ToResponseAsync()
                        .DontMarshall())
            {
                if (!response.HasData) return null;

                var results = CreateResults(response.Data, limit, offset);
                results.Songs = response.Data.Data?.Select(CreateSong).ToList();

                return results;
            }
        }

        public async Task<WebResults> GetArtistAlbumsAsync(string artistToken, int limit = 20,
            string pageToken = null)
        {
            int offset;
            int.TryParse(pageToken, out offset);

            using (
                var response =
                    await new DeezerArtistAlbumsRequest(int.Parse(artistToken))
                        .Limit(limit)
                        .Offset(offset)
                        .ToResponseAsync()
                        .DontMarshall())
            {
                if (!response.HasData) return null;

                var results = CreateResults(response.Data, limit, offset);
                results.Albums = response.Data.Data?.Select(CreateAlbum).ToList();

                return results;
            }
        }

        public override async Task<WebResults> SearchAsync(string query,
            WebResults.Type searchType = WebResults.Type.Song,
            int limit = 20, string pageToken = null)
        {
            int offset;
            int.TryParse(pageToken, out offset);

            using (
                var response =
                    await new DeezerSearchRequest<JToken>(query)
                        .Type(searchType)
                        .Limit(limit)
                        .Offset(offset)
                        .ToResponseAsync()
                        .DontMarshall())
            {
                if (!response.HasData) return null;

                var results = CreateResults(response.Data, limit, offset);

                switch (searchType)
                {
                    case WebResults.Type.Song:
                        results.Songs = response.Data.Data?.Select(CreateSong).ToList();
                        break;
                    case WebResults.Type.Artist:
                        results.Artists = response.Data.Data?.Select(CreateArtist).ToList();
                        break;
                    default:
                        results.Albums = response.Data.Data?.Select(CreateAlbum).ToList();
                        break;
                }

                return results;
            }
        }

        public override async Task<WebAlbum> GetAlbumAsync(string albumToken)
        {
            using (var response = await new DeezerAlbumRequest(albumToken)
                .ToResponseAsync().DontMarshall())
            {
                if (response.HasData)
                    return CreateAlbum(response.Data);

                if (response.HttpResponse.StatusCode == HttpStatusCode.NotFound)
                    throw new ProviderNotFoundException();
                throw new ProviderException();
            }
        }

        public override async Task<WebSong> GetSongAsync(string songToken)
        {
            using (var response = await new DeezerTrackRequest(songToken)
                .ToResponseAsync().DontMarshall())
            {
                if (response.HasData)
                    return CreateSong(response.Data);

                if (response.HttpResponse.StatusCode == HttpStatusCode.NotFound)
                    throw new ProviderNotFoundException();
                throw new ProviderException();
            }
        }

        public override async Task<WebArtist> GetArtistAsync(string artistToken)
        {
            using (var response = await new DeezerArtistRequest(artistToken)
                .ToResponseAsync().DontMarshall())
            {
                if (response.HasData)
                    return CreateArtist(response.Data);

                if (response.HttpResponse.StatusCode == HttpStatusCode.NotFound)
                    throw new ProviderNotFoundException();
                throw new ProviderException();
            }
        }

        public override async Task<WebArtist> GetArtistByNameAsync(string artistName)
        {
            using (var response = await new DeezerArtistRequest(artistName.Replace(" ", "-"))
                .ToResponseAsync().DontMarshall())
            {
                if (response.HasData)
                    return CreateArtist(response.Data);

                if (response.HttpResponse.StatusCode == HttpStatusCode.NotFound)
                    throw new ProviderNotFoundException();
                throw new ProviderException();
            }
        }

        #region helpers

        private WebResults CreateResults<T>(DeezerPageResponse<T> paging, int limit, int currentOffset)
        {
            return new WebResults
            {
                HasMore = paging?.Next != null,
                PageToken = paging == null ? null : (currentOffset + limit).ToString()
            };
        }

        private WebArtist CreateArtist(JToken token)
        {
            var artist = token.ToObject<DeezerArtist>();
            return CreateArtist(artist);
        }

        private WebArtist CreateArtist(DeezerArtist artist)
        {
            return new WebArtist(GetType())
            {
                Name = artist.Name,
                Token = artist.Id.ToString(),
                Artwork = string.IsNullOrEmpty(artist.PictureBig) ? null : new Uri(artist.PictureBig)
            };
        }

        private WebAlbum CreateAlbum(JToken token)
        {
            var album = token.ToObject<DeezerAlbum>();
            return CreateAlbum(album);
        }

        private WebAlbum CreateAlbum(DeezerAlbum album)
        {
            var webAlbum = new WebAlbum(GetType())
            {
                Title = album.Title,
                Token = album.Id.ToString()
            };

            var image = album.CoverBig;
            if (image != null)
                webAlbum.Artwork = new Uri(image);

            if (album.Artist != null)
                webAlbum.Artist = CreateArtist(album.Artist);
            else
                webAlbum.IsPartial = true;

            if (album.Genres != null)
                webAlbum.Genres = album.Genres.Data.Select(p => p.Name).ToList();
            else
                webAlbum.IsPartial = true;

            if (album.Tracks != null)
                webAlbum.Tracks = album.Tracks.Data.Select(CreateSong).ToList();
            else
                webAlbum.IsPartial = true;

            if (album.ReleaseDate != null)
                webAlbum.ReleasedDate = album.ReleaseDate;
            else
                webAlbum.IsPartial = true;

            return webAlbum;
        }

        private WebSong CreateSong(JToken token)
        {
            var song = token.ToObject<DeezerSong>();
            return CreateSong(song);
        }

        private WebSong CreateSong(DeezerSong deezerSong)
        {
            var song = new WebSong(GetType())
            {
                Title = deezerSong.Title,
                Token = deezerSong.Id,
                Artists = new List<WebArtist> {CreateArtist(deezerSong.Artist)},
                Album = deezerSong.Album != null ? CreateAlbum(deezerSong.Album) : null
            };

            if (deezerSong.TrackPosition != null)
                song.TrackNumber = deezerSong.TrackPosition.Value;
            else
                song.IsPartial = true;
            if (deezerSong.DiskNumber != null)
                song.DiskNumber = deezerSong.DiskNumber.Value;
            else
                song.IsPartial = true;

            if (deezerSong.Contributors != null)
                song.Artists = deezerSong.Contributors.Where(p => p.Role == "Main").Select(CreateArtist).ToList();
            else
                song.IsPartial = true;

            return song;
        }

        #endregion
    }
}