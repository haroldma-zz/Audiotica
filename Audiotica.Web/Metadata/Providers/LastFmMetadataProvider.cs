using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Web.Enums;
using Audiotica.Web.Exceptions;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;

namespace Audiotica.Web.Metadata.Providers
{
    public class LastFmMetadataProvider : MetadataProviderWithSearchBase, IExtendedMetadataProvider,
        IChartMetadataProvider
    {
        public LastFmMetadataProvider(ISettingsUtility settingsUtility) : base(settingsUtility)
        {
        }

        public async Task<WebResults> GetTopSongsAsync(int limit = 20, string pageToken = null)
        {
            using (var client = CreateClient())
            {
                var result = await client.Chart
                    .GetTopTracksAsync(pageToken == null
                        ? 1
                        : int.Parse(pageToken), limit);

                if (result.Success)
                {
                    var webResults = CreateResults(result);
                    webResults.Songs = result.Content.Select(CreateSong).ToList();
                    return webResults;
                }

                throw new ProviderException(result.Status.ToString());
            }
        }

        public Task<WebResults> GetTopAlbumsAsync(int limit = 20, string pageToken = null)
        {
            throw new NotImplementedException();
        }

        public async Task<WebResults> GetTopArtistsAsync(int limit = 20, string pageToken = null)
        {
            using (var client = CreateClient())
            {
                var result = await client.Chart
                    .GetTopArtistsAsync(pageToken == null
                        ? 1
                        : int.Parse(pageToken), limit);

                if (result.Success)
                {
                    var webResults = CreateResults(result);
                    webResults.Artists = result.Content.Select(CreateArtist).ToList();
                    return webResults;
                }

                throw new ProviderException(result.Status.ToString());
            }
        }

        public override string DisplayName => "Last.FM";
        public override ProviderSpeed Speed => ProviderSpeed.Average;
        public override ProviderCollectionSize CollectionSize => ProviderCollectionSize.Large;
        public override ProviderCollectionType CollectionQuality => ProviderCollectionType.PrettyMuchEveything;

        public async Task<WebResults> GetRelatedArtistsAsync(string artistToken, int limit = 50, string pageToken = null)
        {
            using (var client = CreateClient())
            {
                var result = await client.Artist.GetSimilarAsync(artistToken, limit: limit);

                if (result.Success)
                {
                    var webResults = CreateResults(result);
                    webResults.Artists = result.Content.Select(CreateArtist).ToList();
                    return webResults;
                }

                throw new ProviderException(result.Status.ToString());
            }
        }

        public async Task<WebResults> GetArtistTopSongsAsync(string artistToken, int limit = 20,
            string pageToken = null)
        {
            using (var client = CreateClient())
            {
                var result =
                    await
                        client.Artist.GetTopTracksAsync(artistToken,
                            page: string.IsNullOrEmpty(pageToken) ? 1 : int.Parse(pageToken));

                if (result.Success)
                {
                    var webResults = CreateResults(result);
                    webResults.Songs = result.Content.Select(CreateSong).ToList();
                    return webResults;
                }

                throw new ProviderException(result.Status.ToString());
            }
        }

        public async Task<WebResults> GetArtistAlbumsAsync(string artistToken, int limit = 20,
            string pageToken = null)
        {
            using (var client = CreateClient())
            {
                var result =
                    await
                        client.Artist.GetTopAlbumsAsync(artistToken,
                            page: string.IsNullOrEmpty(pageToken) ? 1 : int.Parse(pageToken));

                if (result.Success)
                {
                    var webResults = CreateResults(result);
                    webResults.Albums = result.Content.Select(CreateAlbum).ToList();
                    return webResults;
                }

                throw new ProviderException(result.Status.ToString());
            }
        }

        public Task<WebResults> GetArtistNewAlbumsAsync(string artistToken, int limit = 50, string pageToken = null)
        {
            throw new NotImplementedException();
        }

        public override async Task<WebAlbum> GetAlbumAsync(string albumToken)
        {
            using (var client = CreateClient())
            {
                var tokenValues = albumToken.DeTokenize();
                var name = tokenValues[0];
                var artist = tokenValues[1];

                var result = await client.Album.GetInfoAsync(artist, name);

                if (result.Success)
                    return CreateAlbum(result.Content);

                // Something happened, throw exception
                throw new ProviderException(result.Status.ToString());
            }
        }

        public override async Task<WebSong> GetSongAsync(string songToken)
        {
            using (var client = CreateClient())
            {
                var tokenValues = songToken.DeTokenize();
                var name = tokenValues[0];
                var artist = tokenValues[1];

                var result = await client.Track.GetInfoAsync(name, artist);

                if (result.Success)
                {
                    var song = CreateSong(result.Content);

                    // little hack to ensure we get the album and artwork.
                    if (song.Album?.Artwork == null)
                    {
                        try
                        {
                            var album = await GetAlbumAsync(song.Album?.Token ?? songToken);

                            if (song.Album == null || album.Artwork != null)
                                song.Album = album;
                        }
                        catch (ProviderException)
                        {
                            // try searching for it
                            var results = await SearchAsync(song.Title.Append(artist), WebResults.Type.Album, 1);
                            var album = results?.Albums?.FirstOrDefault();
                            if (album?.Artwork != null && song.Title.ToAudioticaSlug().Contains(album.Title.ToAudioticaSlug()) && song.Artists[0].Name.ToAudioticaSlug().Contains(album.Artist.Name.ToAudioticaSlug()))
                                song.Album = album;
                        }

                        song.IsPartial = song.Album != null;
                    }

                    return song;
                }

                // Something happened, throw exception
                throw new ProviderException(result.Status.ToString());
            }
        }

        public override async Task<WebArtist> GetArtistAsync(string artistToken)
        {
            using (var client = CreateClient())
            {
                var result = await client.Artist.GetInfoAsync(artistToken);

                if (result.Success)
                    return CreateArtist(result.Content);

                // Something happened, throw exception
                throw new ProviderException(result.Status.ToString());
            }
        }

        public override Task<WebArtist> GetArtistByNameAsync(string artistName)
        {
            // In this provider we use the artist name as token.
            return GetArtistAsync(artistName);
        }

        private LastfmClient CreateClient() => new LastfmClient(ApiKeys.LastFmId, ApiKeys.LastFmSecret);

        public override async Task<WebResults> SearchAsync(string query,
            WebResults.Type searchType = WebResults.Type.Song,
            int limit = 20, string pageToken = null)
        {
            using (var client = CreateClient())
            {
                var page = string.IsNullOrEmpty(pageToken) ? 1 : int.Parse(pageToken);

                WebResults results;

                switch (searchType)
                {
                    case WebResults.Type.Song:
                    {
                        var response = await client.Track.SearchAsync(query, page, limit);
                        results = CreateResults(response);
                        results.Songs = response.Content.Select(CreateSong).ToList();
                    }
                        break;
                    case WebResults.Type.Artist:
                    {
                        var response = await client.Artist.SearchAsync(query, page, limit);
                        results = CreateResults(response);
                        results.Artists = response.Content.Select(CreateArtist).ToList();
                    }
                        break;
                    default:
                    {
                        var response = await client.Album.SearchAsync(query, page, limit);
                        results = CreateResults(response);
                        results.Albums = response.Content.Select(CreateAlbum).ToList();
                    }
                        break;
                }

                return results;
            }
        }

        #region Helpers

        private WebResults CreateResults<T>(PageResponse<T> paging) where T : new()
        {
            return new WebResults
            {
                HasMore = paging.TotalPages > paging.Page,
                PageToken = $"{paging.Page + 1}"
            };
        }

        private WebSong CreateSong(LastTrack track)
        {
            var song = new WebSong(GetType())
            {
                MetadataProvider = GetType(),
                Title = track.Name,
                Token = new[] {track.Name, track.ArtistName}.Tokenize(),
                // TODO: TrackNumber = 
                Artists = new List<WebArtist>
                {
                    new WebArtist(GetType())
                    {
                        Name = track.ArtistName,
                        Token = track.ArtistName,
                        IsPartial = true
                    }
                }
            };

            if (!string.IsNullOrEmpty(track.AlbumName))
            {
                song.Album =
                    new WebAlbum(GetType())
                    {
                        Title = track.AlbumName,
                        Token = new[] {track.AlbumName, track.ArtistName}.Tokenize(),
                        Artwork = track.Images?.Largest,
                        IsPartial = true
                    };
            }
            else
                song.IsPartial = true;

            return song;
        }

        private WebArtist CreateArtist(LastArtist artist)
        {
            return new WebArtist(GetType())
            {
                Name = artist.Name,
                Token = artist.Name,
                Artwork = artist.MainImage?.Largest
            };
        }

        private WebAlbum CreateAlbum(LastAlbum album)
        {
            var webAlbum = new WebAlbum(GetType())
            {
                Title = album.Name,
                Artist = new WebArtist(GetType()) {Name = album.ArtistName, Token = album.ArtistName, IsPartial = true},
                Token = new[] {album.Name, album.ArtistName}.Tokenize(),
                Artwork = album.Images?.Largest
            };


            if (album.ReleaseDateUtc != null)
            {
                webAlbum.ReleaseDate = album.ReleaseDateUtc.Value.DateTime;
                webAlbum.IsPartial = false;
            }
            else
                webAlbum.IsPartial = true;

            if (album.Tracks != null)
                webAlbum.Tracks = album.Tracks.Select(CreateSong).ToList();
            else
                webAlbum.IsPartial = true;

            return webAlbum;
        }

        #endregion
    }
}