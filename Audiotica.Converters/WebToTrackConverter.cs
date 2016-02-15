using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Extensions;
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

        public async Task<WebSong> FillPartialAsync(WebSong other)
        {
            var provider = _providers.FirstOrDefault(p => p.GetType() == other.MetadataProvider);
            
            if (other.IsPartial)
            {
                var prevAlbum = other.Album;
                var web = await provider.GetSongAsync(other.Token);
                other.SetFrom(web);

                // If the album previously set wasn't a partial, then use that one instead.
                if (prevAlbum != null && !prevAlbum.IsPartial)
                    other.Album = prevAlbum;
            }

            if (other.Album == null)
                other.Album = new WebAlbum(typeof (ILocalMetadataProvider))
                {
                    Title = other.Title,
                    Artist = other.Artists[0]
                };
            else if (other.Album.IsPartial)
            {
                other.Album = await provider.GetAlbumAsync(other.Album.Token);
            }
            if (other.Album.Artist.IsPartial)
                other.Album.Artist = await provider.GetArtistAsync(other.Album.Artist.Token);
            if (other.Artists[0].Token == other.Album.Artist.Token)
                other.Artists[0] = other.Album.Artist;
            else if (other.Artists[0].IsPartial)
                other.Artists[0] = await provider.GetArtistAsync(other.Artists[0].Token);

            return other;
        }

        public async Task<List<WebSong>> FillPartialAsync(IEnumerable<WebSong> others)
        {
            var tasks = others.Select(FillPartialAsync).ToList();
            return (await Task.WhenAll(tasks)).ToList();
        }

        public async Task<Track> ConvertAsync(WebSong other, bool ignoreLibrary = false)
        {
            var conversion = other.PreviousConversion as Track;
            if (conversion != null) return conversion;

            await FillPartialAsync(other);

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
                Year = (uint?)other.Album.ReleaseDate?.Year,
                TrackCount = (uint?)other.Album.Tracks?.Count ?? 1,
                Genres = string.Join("; ", genres.Distinct()),
                AudioWebUri = other.AudioUrl,
                Type = TrackType.Stream
            };

            track.DiscCount = track.DiscNumber;

            var libraryTrack = _libraryService.Find(track);
            other.PreviousConversion = libraryTrack ?? track;

            return ignoreLibrary ? track : libraryTrack ?? track;
        }

        public async Task<List<Track>> ConvertAsync(IEnumerable<WebSong> others, bool ignoreLibrary = false)
        {
            var tasks = others.Select(song => ConvertAsync(song, ignoreLibrary)).ToList();
            return (await Task.WhenAll(tasks)).ToList();
        }
    }
}