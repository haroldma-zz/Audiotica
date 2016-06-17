using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Web.MatchEngine.Interfaces.Providers;
using Audiotica.Web.Services;
using Audiotica.Windows.Services.Interfaces;

namespace Audiotica.Windows.Services.RunTime
{
    internal class LibraryMatchingService : ILibraryMatchingService
    {
        private readonly IAnalyticService _analyticService;
        private readonly IDownloadService _downloadService;
        private readonly ILibraryService _libraryService;
        private readonly IMatchEngineService _matchEngineService;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(5, 5);

        public LibraryMatchingService(ILibraryService libraryService, IMatchEngineService matchEngineService,
            IAnalyticService analyticService, IDownloadService downloadService)
        {
            _libraryService = libraryService;
            _matchEngineService = matchEngineService;
            _analyticService = analyticService;
            _downloadService = downloadService;
        }

        public void OnStartup()
        {
            foreach (var track in _libraryService.Tracks.Where(p => p.Status == TrackStatus.Matching))
                Queue(track);
        }

        public async void Queue(Track track)
        {
            await CreateTaskAsync(track);
        }

        private async Task CreateTaskAsync(Track track)
        {
            await _semaphoreSlim.WaitAsync();

            using (var timer = _analyticService.TrackTimeEvent("Song Matched", new Dictionary<string, object>
            {
                {"title", track.Title},
                {"artists", track.Artists},
                {"album", track.AlbumTitle},
                {"album artist", track.AlbumArtist}
            }))
                try
                {
                    var uri = await _matchEngineService.GetLinkAsync(track.Title, track.DisplayArtist);
                    if (uri != null)
                    {
                        timer.AddProperty("status", "Found match");
                        track.AudioWebUri = uri.ToString();
                        track.Status = TrackStatus.None;
                        await _downloadService.StartDownloadAsync(track);
                    }
                    else
                    {
                        timer.AddProperty("status", "No match");
                        track.Status = TrackStatus.NoMatch;
                    }
                    await _libraryService.UpdateTrackAsync(track);
                }
                catch
                {
                    timer.AddProperty("status", "Error");
                }

            _semaphoreSlim.Release();
        }
    }
}