using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Extensions;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Web.Models;
using Audiotica.Windows.Services.Interfaces;

namespace Audiotica.Windows.Services.RunTime
{
    internal class TrackSaveService : ITrackSaveService
    {
        private readonly ILibraryMatchingService _libraryMatchingService;
        private readonly ILibraryService _libraryService;
        private readonly IConverter<WebSong, Track> _webSongConverter;

        public TrackSaveService(ILibraryService libraryService, IConverter<WebSong, Track> webSongConverter,
            ILibraryMatchingService libraryMatchingService)
        {
            _libraryService = libraryService;
            _webSongConverter = webSongConverter;
            _libraryMatchingService = libraryMatchingService;
        }

        public async Task<Track> SaveAsync(WebSong song)
        {
            var track = await _webSongConverter.ConvertAsync(song, other => song.SetFrom(other));
            await SaveAsync(track);
            return track;
        }

        public async Task SaveAsync(Track track)
        {
            var isMatching = track.AudioWebUri == null && track.AudioLocalUri == null;
            track.Status = isMatching ? Track.TrackStatus.Matching : Track.TrackStatus.None;

            await _libraryService.AddTrackAsync(track);

            // queue it to be matched
            if (isMatching)
                _libraryMatchingService.Queue(track);
        }
    }
}