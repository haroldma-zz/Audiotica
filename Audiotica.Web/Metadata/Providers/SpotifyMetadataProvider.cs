using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Interfaces.Utilities;
using Audiotica.Data.Spotify.Models;
using Audiotica.Web.Enums;
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

            using (var response = await new SpotifySearchRequest(query, searchType.ToString().ToLower().Replace("song", "track"))
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

        public override Task<WebAlbum> GetAlbumAsync(string albumToken)
        {
            throw new NotImplementedException();
        }

        public override Task<WebSong> GetSongAsync(string songToken)
        {
            throw new NotImplementedException();
        }

        public override Task<WebArtist> GetArtistAsync(string artistToken)
        {
            throw new NotImplementedException();
        }

        public override Task<List<WebSong>> GetArtistTopSongsAsync(string artistToken)
        {
            throw new NotImplementedException();
        }

        public override Task<List<WebAlbum>> GetArtistTopAlbumsAsync(string artistToken)
        {
            throw new NotImplementedException();
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
            var webAlbum = CreateArtist(artist as SimpleArtist);

            webAlbum.IsPartial = false;
            var image = artist.Images?.FirstOrDefault();
            if (image != null)
                webAlbum.Artwork = new Uri(image.Url);

            return webAlbum;
        }

        private WebSong CreateSong(SimpleTrack track)
        {
            return new WebSong
            {
                Title = track.Name,
                Token = track.Id,
                Artist = CreateArtist(track is FullTrack ? ((FullTrack) track).Artist : track.Artist)
            };
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

            DateTime released;
            if (DateTime.TryParse(album.ReleaseDate, out released))
                webAlbum.ReleasedDate = released;

            return webAlbum;
        }

        #endregion
    }
}