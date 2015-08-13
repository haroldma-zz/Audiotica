using System;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Exceptions;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Web.MatchEngine.Interfaces.Providers;
using Audiotica.Web.Models;
using Audiotica.Windows.Services.Interfaces;

namespace Audiotica.Windows.Services.RunTime
{
    public class TrackSaveService : ITrackSaveService
    {
        private readonly ILibraryService _libraryService;
        private readonly IMatchEngineService _matchEngineService;
        private readonly IConverter<WebSong, Track> _webSongConverter;

        public TrackSaveService(ILibraryService libraryService, IConverter<WebSong, Track> webSongConverter,
            IMatchEngineService matchEngineService)
        {
            _libraryService = libraryService;
            _webSongConverter = webSongConverter;
            _matchEngineService = matchEngineService;
        }

        public async Task<Track> SaveAsync(WebSong song, Action<WebSong> saveChanges = null)
        {
            var track = await _webSongConverter.ConvertAsync(song, webSong => saveChanges?.Invoke(webSong));
            return await SaveAsync(track);
        }

        public async Task<Track> SaveAsync(Track track)
        {
            if (track.AudioWebUri == null && track.AudioLocalUri == null)
            {
                // TODO: match in the background
                var uri = await _matchEngineService.GetLinkAsync(track.Title, track.DisplayArtist);
                if (uri == null) throw new NoMatchFoundException();
                track.AudioWebUri = uri.ToString();
            }

            track.Status = Track.TrackStatus.None;
            return await _libraryService.AddTrackAsync(track);
        }
    }
}