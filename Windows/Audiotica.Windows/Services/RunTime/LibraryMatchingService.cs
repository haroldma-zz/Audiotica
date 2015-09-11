using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Web.MatchEngine.Interfaces.Providers;
using Audiotica.Windows.Services.Interfaces;

namespace Audiotica.Windows.Services.RunTime
{
    internal class LibraryMatchingService : ILibraryMatchingService
    {
        private readonly IInsightsService _insightsService;
        private readonly ILibraryService _libraryService;
        private readonly IMatchEngineService _matchEngineService;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(5, 5);

        public LibraryMatchingService(ILibraryService libraryService, IMatchEngineService matchEngineService,
            IInsightsService insightsService)
        {
            _libraryService = libraryService;
            _matchEngineService = matchEngineService;
            _insightsService = insightsService;
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

            using (var timer = _insightsService.TrackTimeEvent("SongMatched", new Dictionary<string, string>
            {
                {"Title", track.Title},
                {"Artists", track.Artists},
                {"Album", track.AlbumTitle},
                {"Album artist", track.AlbumArtist}
            }))
                try
                {
                    var uri = await _matchEngineService.GetLinkAsync(track.Title, track.DisplayArtist);
                    if (uri != null)
                    {
                        timer.AddProperty("Status", "Found match");
                        track.AudioWebUri = uri.ToString();
                        track.Status = TrackStatus.None;
                        await _libraryService.UpdateTrackAsync(track);
                    }
                    else
                    {
                        timer.AddProperty("Status", "No match");
                        track.Status = TrackStatus.NoMatch;
                        await _libraryService.UpdateTrackAsync(track);
                    }
                }
                catch
                {
                    timer.AddProperty("Status", "Error");
                }

            _semaphoreSlim.Release();
        }
    }
}