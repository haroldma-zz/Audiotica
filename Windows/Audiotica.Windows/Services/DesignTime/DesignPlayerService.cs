using System;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Audiotica.Core.Common;
using Audiotica.Database.Models;
using Audiotica.Web.Models;

namespace Audiotica.Core.Windows.Services
{
    public class DesignPlayerService : IPlayerService
    {
        public bool IsBackgroundTaskRunning { get; }
        public MediaPlayerState CurrentState { get; set; }
        public string CurrentQueueId { get; }
        public OptimizedObservableCollection<QueueTrack> PlaybackQueue { get; }
        public event EventHandler<MediaPlayerState> MediaStateChanged;
        public event EventHandler<string> TrackChanged;

        public Task<bool> StartBackgroundTaskAsync()
        {
            throw new NotImplementedException();
        }

        public void PlayOrPause()
        {
            throw new NotImplementedException();
        }

        public Task<QueueTrack> AddAsync(Track track, int position = -1)
        {
            throw new NotImplementedException();
        }

        public Task<QueueTrack> AddAsync(WebSong webSong, int position = -1)
        {
            throw new NotImplementedException();
        }

        public Task<QueueTrack> AddUpNextAsync(Track track)
        {
            throw new NotImplementedException();
        }

        public Task<QueueTrack> AddUpNextAsync(WebSong webSong)
        {
            throw new NotImplementedException();
        }

        public void Play(QueueTrack queue)
        {
            throw new NotImplementedException();
        }

        public void Previous()
        {
            throw new NotImplementedException();
        }

        public void Next()
        {
            throw new NotImplementedException();
        }

        public void Resuming()
        {
            throw new NotImplementedException();
        }

        public void Suspending()
        {
            throw new NotImplementedException();
        }
    }
}