using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Web.Enums;
using Audiotica.Web.Exceptions;
using Audiotica.Web.Http.Requets.Metadata.Spotify;
using Audiotica.Web.Http.Requets.Metadata.Spotify.Models;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;

namespace Audiotica.Web.Metadata.Providers
{
    public class SpotifyMetadataProvider : MetadataProviderWithSearchBase,
        IExtendedMetadataProvider, IChartMetadataProvider
    {
        public SpotifyMetadataProvider(ISettingsUtility settingsUtility) : base(settingsUtility)
        {
        }

        public async Task<WebResults> GetTopSongsAsync(int limit = 20, string pageToken = null)
        {
            using (var response = await new SpotifyChartRequest().ToResponseAsync())
            {
                if (response.HasData)
                    return new WebResults
                    {
                        HasMore = false,
                        Songs = response.Data.Tracks.Select(CreateSong).Take(limit).ToList()
                    };

                throw new ProviderException();
            }
        }

        public Task<WebResults> GetTopAlbumsAsync(int limit = 20, string pageToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<WebResults> GetTopArtistsAsync(int limit = 20, string pageToken = null)
        {
            throw new NotImplementedException();
        }

        public override string DisplayName => "Spotify";
        public override ProviderSpeed Speed => ProviderSpeed.Fast;
        public override ProviderCollectionSize CollectionSize => ProviderCollectionSize.Large;
        public override ProviderCollectionType CollectionQuality => ProviderCollectionType.GoodStuff;

        public Task<WebResults> GetRelatedArtistsAsync(string artistToken, int limit = 50, string pageToken = null)
        {
            throw new NotImplementedException();
        }

        public async Task<WebResults> GetArtistTopSongsAsync(string artistToken, int limit = 20,
            string pageToken = null)
        {
            using (var response = await new SpotifyArtistTopTracksRequest(artistToken)
                .ToResponseAsync())
            {
                if (!response.HasData)
                {
                    throw new ProviderException();
                }
                if (response.Data.HasError())
                    throw new ProviderException(response.Data.ErrorResponse.Message);

                var results = new WebResults {Songs = response.Data.Tracks.Select(CreateSong).Take(limit).ToList()};
                return results;
            }
        }

        public async Task<WebResults> GetArtistAlbumsAsync(string artistToken, int limit = 20,
            string pageToken = null)
        {
            using (var response = await new SpotifyArtistAlbumsRequest(artistToken)
                .Offset(pageToken == null ? 0 : int.Parse(pageToken))
                .Limit(limit)
                .Types(AlbumType.Album | AlbumType.Compilation | AlbumType.Single)
                .ToResponseAsync())
            {
                if (!response.HasData)
                {
                    throw new ProviderException();
                }
                if (response.Data.HasError())
                    throw new ProviderException(response.Data.ErrorResponse.Message);

                var results = CreateResults(response.Data);
                results.Albums = response.Data.Items.Select(CreateAlbum).Distinct(new WebAlbum.Comparer()).ToList();
                return results;
            }
        }

        public Task<WebResults> GetArtistNewAlbumsAsync(string artistToken, int limit = 50, string pageToken = null)
        {
            throw new NotImplementedException();
        }

        public override async Task<WebAlbum> GetAlbumAsync(string albumToken)
        {
            // TODO: see if the endpoint returns the full track list
            using (var response = await new SpotifyAlbumRequest(albumToken).ToResponseAsync())
            {
                if (response.HasData)
                {
                    if (response.Data.HasError())
                        throw new ProviderException(response.Data.ErrorResponse.Message);
                    return CreateAlbum(response.Data);
                }
                throw new ProviderException();
            }
        }

        public override async Task<WebSong> GetSongAsync(string songToken)
        {
            using (var response = await new SpotifyTrackRequest(songToken).ToResponseAsync())
            {
                if (response.HasData)
                {
                    if (response.Data.HasError())
                        throw new ProviderException(response.Data.ErrorResponse.Message);
                    return CreateSong(response.Data);
                }
                throw new ProviderException();
            }
        }

        public override async Task<WebArtist> GetArtistAsync(string artistToken)
        {
            using (var response = await new SpotifyArtistRequest(artistToken).ToResponseAsync())
            {
                if (response.HasData)
                {
                    if (response.Data.HasError())
                        throw new ProviderException(response.Data.ErrorResponse.Message);
                    return CreateArtist(response.Data);
                }
                throw new ProviderException();
            }
        }

        public override async Task<WebArtist> GetArtistByNameAsync(string artistName)
        {
            // No api for getting by artist name, so do a search to find the id.
            var results = await SearchAsync(artistName, WebResults.Type.Artist, 1);
            var artist = results.Artists?.FirstOrDefault(p => string.Equals(p.Name, artistName,
                StringComparison.CurrentCultureIgnoreCase));
            if (artist == null)
                throw new ProviderException("Not found.");
            return artist;
        }

        public override async Task<WebResults> SearchAsync(string query,
            WebResults.Type searchType = WebResults.Type.Song, int limit = 20, string pageToken = null)
        {
            int offset;
            int.TryParse(pageToken, out offset);

            using (var response =
                await new SpotifySearchRequest(query)
                    .Type(searchType)
                    .Limit(limit)
                    .Offset(offset)
                    .ToResponseAsync()
                    .DontMarshall())
            {
                if (!response.HasData) return null;
                if (response.Data.HasError())
                    throw new ProviderException(response.Data.ErrorResponse.Message);

                WebResults results;

                switch (searchType)
                {
                    case WebResults.Type.Song:
                        results = CreateResults(response.Data.Tracks);
                        results.Songs = response.Data.Tracks?.Items?.Select(CreateSong).ToList();
                        break;
                    case WebResults.Type.Artist:
                        results = CreateResults(response.Data.Artists);
                        results.Artists = response.Data.Artists?.Items?.Select(CreateArtist).ToList();
                        break;
                    default:
                        results = CreateResults(response.Data.Albums);
                        results.Albums =
                            response.Data.Albums?.Items?.Select(CreateAlbum).Distinct(new WebAlbum.Comparer()).ToList();
                        break;
                }

                return results;
            }
        }

        #region Helpers

        private WebResults CreateResults<T>(Paging<T> paging)
        {
            return new WebResults
            {
                HasMore = paging?.Next != null,
                PageToken = (paging?.Offset + paging?.Limit).ToString()
            };
        }

        private WebArtist CreateArtist(SimpleArtist artist)
        {
            return new WebArtist(GetType())
            {
                Name = artist.Name,
                Token = artist.Id,
                IsPartial = true
            };
        }

        private WebArtist CreateArtist(FullArtist artist)
        {
            var webArtist = CreateArtist(artist as SimpleArtist);

            webArtist.IsPartial = false;
            var image = artist.Images?.FirstOrDefault();
            if (image != null)
                webArtist.Artwork = new Uri(image.Url);

            return webArtist;
        }

        private WebSong CreateSong(ChartTrack track)
        {
            var song = new WebSong(GetType())
            {
                Title = track.Name,
                Token = track.Id,
                IsPartial = true,
                Artists = new List<WebArtist>
                {
                    new WebArtist(GetType())
                    {
                        Name = track.ArtistName,
                        IsPartial = true,
                        Token = track.ArtistId
                    }
                },
                Album = new WebAlbum(GetType())
                {
                    Title = track.AlbumName,
                    IsPartial = true,
                    Token = track.AlbumId,
                    Artwork = new Uri(track.ArtworkUrl)
                }
            };


            return song;
        }

        private WebSong CreateSong(SimpleTrack track)
        {
            var song = new WebSong(GetType())
            {
                Title = track.Name,
                Token = track.Id,
                IsPartial = true,
                TrackNumber = track.TrackNumber,
                DiskNumber = track.DiscNumber
            };

            var full = track as FullTrack;
            if (full != null)
            {
                song.IsPartial = false;

                if (full.Artists != null)
                    song.Artists = full.Artists.Select(CreateArtist).ToList();
                else
                    song.IsPartial = true;

                if (full.Album != null)
                    song.Album = CreateAlbum(full.Album);
                else
                    song.IsPartial = true;
            }
            else
            {
                if (track.Artist != null)
                    song.Artists = new List<WebArtist> {CreateArtist(track.Artist)};
            }

            return song;
        }

        private WebSong CreateSong(FullTrack track)
        {
            var webTrack = CreateSong(track as SimpleTrack);
            return webTrack;
        }

        private WebAlbum CreateAlbum(SimpleAlbum album)
        {
            var webAlbum = new WebAlbum(GetType())
            {
                Title = album.Name,
                Token = album.Id,
                IsPartial = true
            };

            var image = album.Images?.FirstOrDefault();
            if (image != null)
                webAlbum.Artwork = new Uri(image.Url);

            return webAlbum;
        }

        private WebAlbum CreateAlbum(FullAlbum album)
        {
            var webAlbum = CreateAlbum(album as SimpleAlbum);

            webAlbum.IsPartial = false;
            webAlbum.Tracks = album.Tracks?.Items?.Select(CreateSong).ToList();
            webAlbum.Artist = CreateArtist(album.Artist);
            webAlbum.Genres = album.Genres;

            DateTime released;
            if (DateTime.TryParse(album.ReleaseDate, out released))
                webAlbum.ReleaseDate = released;

            return webAlbum;
        }

        #endregion
    }
}