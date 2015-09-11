using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Database.Extensions;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Web.Extensions;
using Audiotica.Web.Models;
using Audiotica.Windows.Services.Interfaces;

namespace Audiotica.Windows.Services.RunTime
{
    internal class TrackSaveService : ITrackSaveService
    {
        private readonly IInsightsService _insightsService;
        private readonly IStorageUtility _storageUtility;
        private readonly ILibraryService _libraryService;
        private readonly ILibraryMatchingService _matchingService;
        private readonly IConverter<WebSong, Track> _webSongConverter;

        public TrackSaveService(ILibraryService libraryService, IConverter<WebSong, Track> webSongConverter,
            ILibraryMatchingService matchingService, IInsightsService insightsService, IStorageUtility storageUtility)
        {
            _libraryService = libraryService;
            _webSongConverter = webSongConverter;
            _matchingService = matchingService;
            _insightsService = insightsService;
            _storageUtility = storageUtility;
        }

        public async Task<Track> SaveAsync(WebSong song)
        {
            var track = await _webSongConverter.ConvertAsync(song);
            await SaveAsync(track);
            return track;
        }

        public async Task SaveAsync(Track track)
        {
            using (_insightsService.TrackTimeEvent("SongSaved", new Dictionary<string, string>
            {
                {"Track type", track.Type.ToString()},
                {"Title", track.Title},
                {"Artists", track.Artists},
                {"Album", track.AlbumTitle},
                {"Album artist", track.AlbumArtist}
            }))
            {
                var isMatching = track.AudioWebUri == null && track.AudioLocalUri == null;
                track.Status = isMatching ? TrackStatus.Matching : TrackStatus.None;

                // Download artwork
                await DownloadAlbumArtworkAsync(track);
                await DownloadArtistArtworkAsync(track);

                await _libraryService.AddTrackAsync(track);

                // proccess it
                if (isMatching)
                    _matchingService.Queue(track);
            }
        }

        private async Task DownloadArtistArtworkAsync(Track track)
        {
            var artistHash = track.GetArtistHash();
            const string prefix = "ms-appdata:///local/";
            var path = $"Library/Images/Artists/{artistHash}.png";
            var uri = prefix + path;
            
            var exists = _libraryService.Tracks.Any(p => p.ArtistArtworkUri.EqualsIgnoreCase(uri));

            if (!exists)
            {
                if (string.IsNullOrEmpty(track.ArtistArtworkUri))
                    return;

                using (var response = await track.ArtistArtworkUri.ToUri().GetAsync())
                    if (response.IsSuccessStatusCode)
                        using (var stream = await response.Content.ReadAsStreamAsync())
                            await _storageUtility.WriteStreamAsync(path, stream);
                    else return;
            }

            track.ArtistArtworkUri = uri;
        }

        private async Task DownloadAlbumArtworkAsync(Track track)
        {
            var albumHash = track.GetAlbumHash();
            const string prefix = "ms-appdata:///local/";
            var path = $"Library/Images/Albums/{albumHash}.png";
            var uri = prefix + path;

            var exists = _libraryService.Tracks.Any(p => p.ArtworkUri.EqualsIgnoreCase(uri));

            if (!exists)
            {
                if (string.IsNullOrEmpty(track.ArtworkUri))
                    return;

                using (var response = await track.ArtworkUri.ToUri().GetAsync())
                    if (response.IsSuccessStatusCode)
                        using (var stream = await response.Content.ReadAsStreamAsync())
                            await _storageUtility.WriteStreamAsync(path, stream);
                    else return;
            }

            track.ArtworkUri = uri;
        }
    }
}