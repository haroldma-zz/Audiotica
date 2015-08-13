using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Web.MatchEngine.Interfaces.Providers;
using Audiotica.Windows.Services.Interfaces;

namespace Audiotica.Windows.Services.RunTime
{
    internal class LibraryMatchingService : ILibraryMatchingService
    {
        private const int MaxParallels = 5;
        private readonly ILibraryService _libraryService;
        private readonly IMatchEngineService _matchEngineService;
        private readonly List<Task> _matchingTasks = new List<Task>();

        public LibraryMatchingService(ILibraryService libraryService, IMatchEngineService matchEngineService)
        {
            _libraryService = libraryService;
            _matchEngineService = matchEngineService;
        }

        public bool IsMatching { get; private set; }

        public void OnStartup()
        {
            _matchingTasks.AddRange(
                _libraryService.Tracks.Where(p => p.Status == Track.TrackStatus.Matching).Select(CreateTask));
            RunTasks();
        }

        public void Queue(Track track)
        {
            _matchingTasks.Add(CreateTask(track));
            if (!IsMatching)
                RunTasks();
        }

        private async Task CreateTask(Track track)
        {
            try
            {
                var uri = await _matchEngineService.GetLinkAsync(track.Title, track.DisplayArtist);
                if (uri != null)
                {
                    track.AudioWebUri = uri.ToString();
                    track.Status = Track.TrackStatus.None;
                    await _libraryService.UpdateTrackAsync(track);
                }
            }
            catch
            {
                // ignored
            }
        }

        private async void RunTasks()
        {
            IsMatching = true;

            while (_matchingTasks.Count > 0)
            {
                var running = new List<Task>();

                var min = Math.Min(_matchingTasks.Count, MaxParallels);
                for (var index = 0; index < min; index++)
                {
                    var task = _matchingTasks[index];
                    running.Add(task);
                }

                running.ForEach(task => _matchingTasks.Remove(task));

                await Task.WhenAll(running);
            }

            IsMatching = false;
        }
    }
}