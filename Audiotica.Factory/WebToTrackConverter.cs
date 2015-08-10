using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Helpers;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Metadata.Providers;
using Audiotica.Web.Models;

namespace Audiotica.Factory
{
    public class WebToTrackConverter : IConverter<WebSong, Track>
    {
        private readonly IMetadataProvider[] _providers;
        private readonly ILibraryService _libraryService;

        public WebToTrackConverter(IMetadataProvider[] providers, ILibraryService libraryService)
        {
            _providers = providers;
            _libraryService = libraryService;
        }

        public async Task<Track> ConvertAsync(WebSong other, Action<WebSong> saveChanges = null)
        {
            var provider = _providers.FirstOrDefault(p => p.GetType() == other.MetadataProvider);

            if (other.IsPartial)
                other = await provider.GetSongAsync(other.Token);
            if (other.Album == null)
                other.Album = new WebAlbum(typeof (ILocalMetadataProvider))
                {
                    Title = other.Title,
                    Artist = other.Artists[0]
                };
            else if (other.Album.IsPartial)
                other.Album = await provider.GetAlbumAsync(other.Album.Token);
            if (other.Album.Artist.IsPartial)
                other.Album.Artist = await provider.GetArtistAsync(other.Album.Artist.Token);
            if (other.Artists[0].Token != other.Album.Artist.Token
                && other.Artists[0].IsPartial)
                other.Artists[0] = await provider.GetArtistAsync(other.Artists[0].Token);

            var track = new Track
            {
                Title = other.Title,
                Artists = string.Join("; ", other.Artists.Select(p => p.Name)),
                AlbumTitle = other.Album.Title,
                AlbumArtist = other.Album.Artist.Name,
                ArtworkUri = other.Album.Artwork,
                ArtistArtworkUri = other.Artists[0].Artwork,
                DisplayArtist = other.Artists[0].Name,
                TrackNumber = other.TrackNumber != 0 ? other.TrackNumber : 1,
                DiscNumber = other.DiscNumber != 0 ? other.DiscNumber : 1,
                DiscCount = 1,
                Year = other.Album.ReleasedDate?.Year,
                TrackCount = other.Album.Tracks?.Count ?? 1,
                Genres = other.Genres,
                Type = Track.TrackType.Stream
            };

            other.PreviousConversion = track;
            saveChanges?.Invoke(other);

            return track;
        }

        public async Task<List<Track>> ConvertAsync(IEnumerable<WebSong> others)
        {
            var tasks = others.Select(LibraryConvert).ToList();
            return (await Task.WhenAll(tasks)).ToList();
        }

        private async Task<Track> LibraryConvert(WebSong webSong)
        {
            var track = await ConvertAsync(webSong);
            var libraryTrack = track; //TODO: _libraryService
            return libraryTrack ?? track;
        }
    }
}