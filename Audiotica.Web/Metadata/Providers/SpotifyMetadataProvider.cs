using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Data.Spotify.Models;
using Audiotica.Web.Enums;
using Audiotica.Web.Exceptions;
using Audiotica.Web.Http.Requets.Metadata.Spotify;
using Audiotica.Web.Models;

namespace Audiotica.Web.Metadata.Providers
{
    public class SpotifyMetadataProvider : MetadataProviderBase
    {
        public SpotifyMetadataProvider(ISettingsUtility settingsUtility) : base(settingsUtility)
        {
        }

        public override string DisplayName => "Spotify";
        public override ProviderSpeed Speed => ProviderSpeed.Fast;
        public override ProviderCollectionSize CollectionSize => ProviderCollectionSize.Large;
        public override ProviderCollectionType CollectionType => ProviderCollectionType.Mainstream;

        public override async Task<WebResults> SearchAsync(string query,
            WebResults.Type searchType = WebResults.Type.Song, int limit = 10, string pagingToken = null)
        {
            int offset;
            int.TryParse(pagingToken, out offset);

            using (
                var response =
                    await new SpotifySearchRequest(query, searchType.ToString().ToLower().Replace("song", "track"))
                        .Limit(limit)
                        .Offset(offset)
                        .ToResponseAsync()
                        .DontMarshall())
            {
                if (!response.HasData) return null;

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
                        results.Albums = response.Data.Albums?.Items?.Select(CreateAlbum).ToList();
                        break;
                }

                return results;
            }
        }

        public override async Task<WebAlbum> GetAlbumAsync(string albumToken)
        {
            // TODO: see if the endpoint returns the full track list
            using (var response = await new SpotifyAlbumRequest(albumToken).ToResponseAsync())
            {
                if (response.HasData)
                    return CreateAlbum(response.Data);
                if (response.HttpResponse.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw new ProviderException();
            }
        }

        public override async Task<WebSong> GetSongAsync(string songToken)
        {
            using (var response = await new SpotifyTrackRequest(songToken).ToResponseAsync())
            {
                if (response.HasData)
                    return CreateSong(response.Data);
                if (response.HttpResponse.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw new ProviderException();
            }
        }

        public override async Task<WebArtist> GetArtistAsync(string artistToken)
        {
            using (var response = await new SpotifyArtistRequest(artistToken).ToResponseAsync())
            {
                if (response.HasData)
                    return CreateArtist(response.Data);
                if (response.HttpResponse.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw new ProviderException();
            }
        }

        public override async Task<WebResults> GetArtistTopSongsAsync(string artistToken, int limit = 50,
            string pageToken = null)
        {
            using (var response = await new SpotifyArtistTopTracksRequest(artistToken)
                .Offset(pageToken == null ? 0 : int.Parse(pageToken))
                .Limit(limit)
                .ToResponseAsync())
            {
                if (!response.HasData)
                {
                    if (response.HttpResponse.StatusCode == HttpStatusCode.NotFound)
                        return null;
                    throw new ProviderException();
                }

                var results = CreateResults(response.Data);
                results.Songs = response.Data.Items.Select(CreateSong).ToList();
                return results;
            }
        }

        public override async Task<WebResults> GetArtistAlbumsAsync(string artistToken, int limit = 50,
            string pageToken = null)
        {
            using (var response = await new SpotifyArtistAlbumsRequest(artistToken)
                .Offset(pageToken == null ? 0 : int.Parse(pageToken))
                .Limit(limit)
                .ToResponseAsync())
            {
                if (!response.HasData)
                {
                    if (response.HttpResponse.StatusCode == HttpStatusCode.NotFound)
                        return null;
                    throw new ProviderException();
                }

                var results = CreateResults(response.Data);
                results.Albums = response.Data.Items.Select(CreateAlbum).ToList();
                return results;
            }
        }

        public override Task<string> GetLyricAsync(string song, string artist)
        {
            // Spotify doesn't provide lyrics.
            return Task.FromResult(string.Empty);
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
            return new WebArtist
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

        private WebSong CreateSong(SimpleTrack track)
        {
            var song = new WebSong
            {
                Title = track.Name,
                Token = track.Id,
                IsPartial = true,
                TrackNumber = track.TrackNumber
            };

            var full = track as FullTrack;
            if (full != null)
            {
                song.IsPartial = false;

                if (full.Artist != null)
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
            webTrack.Album = CreateAlbum(track.Album);
            return webTrack;
        }

        private WebAlbum CreateAlbum(SimpleAlbum album)
        {
            var webAlbum = new WebAlbum
            {
                Name = album.Name,
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

            DateTime released;
            if (DateTime.TryParse(album.ReleaseDate, out released))
                webAlbum.ReleasedDate = released;

            return webAlbum;
        }

        #endregion
    }
}