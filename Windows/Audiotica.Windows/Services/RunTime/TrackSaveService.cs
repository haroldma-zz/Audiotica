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
using Audiotica.Web.Services;
using Audiotica.Windows.Services.Interfaces;
using TagLib;

namespace Audiotica.Windows.Services.RunTime
{
    internal class TrackSaveService : ITrackSaveService
    {
        private readonly IDownloadService _downloadService;
        private readonly IAnalyticService _analyticService;
        private readonly ILibraryService _libraryService;
        private readonly ILibraryMatchingService _matchingService;
        private readonly IStorageUtility _storageUtility;
        private readonly IConverter<WebSong, Track> _webSongConverter;

        public TrackSaveService(
            ILibraryService libraryService,
            IConverter<WebSong, Track> webSongConverter,
            ILibraryMatchingService matchingService,
            IAnalyticService analyticService,
            IStorageUtility storageUtility,
            IDownloadService downloadService)
        {
            _libraryService = libraryService;
            _webSongConverter = webSongConverter;
            _matchingService = matchingService;
            _analyticService = analyticService;
            _storageUtility = storageUtility;
            _downloadService = downloadService;
        }

        public async Task InternalSaveAsync(Track track, byte[] albumData, byte[] artistData)
        {
            using (_analyticService.TrackTimeEvent("Song Saved",
                new Dictionary<string, object>
                {
                    { "track type", track.Type.ToString() },
                    { "title", track.Title },
                    { "artists", track.Artists },
                    { "album", track.AlbumTitle },
                    { "album artist", track.AlbumArtist }
                }))
            {
                var isMatching = track.AudioWebUri == null && track.AudioLocalUri == null;
                track.Status = isMatching ? TrackStatus.Matching : TrackStatus.None;

                // Download artwork
                await DownloadAlbumArtworkAsync(track, albumData);
                await DownloadArtistArtworkAsync(track, artistData);

                await _libraryService.AddTrackAsync(track);

                // proccess it
                if (isMatching)
                {
                    _matchingService.Queue(track);
                }
                else if (track.AudioLocalUri == null)
                {
                    await _downloadService.StartDownloadAsync(track);
                }
            }
        }

        public async Task<Track> SaveAsync(WebSong song)
        {
            var track = await _webSongConverter.ConvertAsync(song);
            await SaveAsync(track);
            return track;
        }

        public Task SaveAsync(Track track)
        {
            return InternalSaveAsync(track, null, null);
        }

        public Task SaveAsync(Track track, Tag tag)
        {
            var albumArtwork = tag.Pictures.FirstOrDefault(p => p.Type == PictureType.FrontCover);
            var artistArtwork = tag.Pictures.FirstOrDefault(p => p.Type == PictureType.Artist);
            return InternalSaveAsync(track, albumArtwork?.Data?.Data, artistArtwork?.Data?.Data);
        }

        private async Task DownloadAlbumArtworkAsync(Track track, byte[] data)
        {
            var albumHash = track.GetAlbumHash();
            const string prefix = "ms-appdata:///local/";
            var path = $"Library/Images/Albums/{albumHash}.png";
            var uri = prefix + path;

            var exists = _libraryService.Tracks.Any(p => p.ArtworkUri.EqualsIgnoreCase(uri));

            if (!exists)
            {
                if (data != null)
                {
                    if (!await SaveArtworkAsync(data, path))
                    {
                        return;
                    }
                }
                else if (!await DownloadArtworkAsync(track.ArtworkUri, path))
                {
                    return;
                }
            }

            track.ArtworkUri = uri;
        }

        private async Task DownloadArtistArtworkAsync(Track track, byte[] data)
        {
            var artistHash = track.GetArtistHash();
            const string prefix = "ms-appdata:///local/";
            var path = $"Library/Images/Artists/{artistHash}.png";
            var uri = prefix + path;

            var exists = _libraryService.Tracks.Any(p => p.ArtistArtworkUri.EqualsIgnoreCase(uri));

            if (!exists)
            {
                if (data != null)
                {
                    if (!await SaveArtworkAsync(data, path))
                    {
                        return;
                    }
                }
                else if (!await DownloadArtworkAsync(track.ArtistArtworkUri, path))
                {
                    return;
                }
            }

            track.ArtistArtworkUri = uri;
        }

        private async Task<bool> DownloadArtworkAsync(string uri, string path)
        {
            if (string.IsNullOrEmpty(uri) || !uri.StartsWith("http"))
            {
                return false;
            }

            // make sure it doesn't exists (when track is deleted, artwork won't be deleted until next startup)
            if (!await _storageUtility.ExistsAsync(path))
            {
                using (var response = await uri.ToUri().GetAsync())
                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            await _storageUtility.WriteStreamAsync(path, stream);
                            return true;
                        }
                    }
            }
            return false;
        }

        private async Task<bool> SaveArtworkAsync(byte[] data, string path)
        {
            // make sure it doesn't exists (when track is deleted, artwork won't be deleted until next startup)
            if (await _storageUtility.ExistsAsync(path))
            {
                return false;
            }
            await _storageUtility.WriteBytesAsync(path, data);
            return true;
        }
    }
}