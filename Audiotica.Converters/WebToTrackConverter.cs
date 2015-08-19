using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Web.Extensions;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Metadata.Providers;
using Audiotica.Web.Models;

namespace Audiotica.Converters
{
    public class WebToTrackConverter : IConverter<WebSong, Track>
    {
        private readonly ILibraryService _libraryService;
        private readonly List<IBasicMetadataProvider> _providers;

        public WebToTrackConverter(IEnumerable<IMetadataProvider> providers, ILibraryService libraryService)
        {
            _providers = providers.FilterAndSort<IBasicMetadataProvider>();
            _libraryService = libraryService;
        }

        public async Task<Track> ConvertAsync(WebSong other, Action<WebSong> saveChanges = null)
        {
            var conversion = other.PreviousConversion as Track;
            if (conversion != null) return conversion;

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
            if (other.Artists[0].Token == other.Album.Artist.Token)
                other.Artists[0] = other.Album.Artist;
            else if (other.Artists[0].IsPartial)
                other.Artists[0] = await provider.GetArtistAsync(other.Artists[0].Token);

            // some providers only have genres in the album object, others on the song
            var genres = new List<string>();
            if (other.Genres != null)
                genres = other.Genres;
            if (other.Album.Genres != null)
                genres.AddRange(other.Album.Genres);

            var track = new Track
            {
                Title = other.Title,
                Artists = string.Join("; ", other.Artists.Select(p => p.Name)),
                AlbumTitle = other.Album.Title,
                AlbumArtist = other.Album.Artist.Name,
                ArtworkUri = other.Album.Artwork?.ToString(),
                ArtistArtworkUri = other.Artists[0].Artwork?.ToString(),
                DisplayArtist = other.Artists[0].Name,
                TrackNumber = other.TrackNumber != 0 ? other.TrackNumber : 1,
                DiscNumber = other.DiskNumber != 0 ? other.DiskNumber : 1,
                Year = other.Album.ReleasedDate?.Year,
                TrackCount = other.Album.Tracks?.Count ?? 1,
                Genres = string.Join("; ", genres.Distinct()),
                Type = Track.TrackType.Stream
            };

            track.DiscCount = track.DiscNumber;
            other.PreviousConversion = track;
            saveChanges?.Invoke(other);

            var libraryTrack = _libraryService.Find(track);
            return libraryTrack ?? track;
        }

        public async Task<List<Track>> ConvertAsync(IEnumerable<WebSong> others)
        {
            var tasks = others.Select(p => ConvertAsync(p)).ToList();
            return (await Task.WhenAll(tasks)).ToList();
        }
    }
}