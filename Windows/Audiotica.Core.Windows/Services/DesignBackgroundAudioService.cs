using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Audiotica.Database.Models;

namespace Audiotica.Core.Windows.Services
{
    public class DesignBackgroundAudioService : IBackgroundAudioService
    {
        public bool IsBackgroundTaskRunning { get; }
        public MediaPlayerState CurrentState { get; set; }
        public int CurrentQueueId { get; }
        public event EventHandler<MediaPlayerState> MediaStateChanged;
        public event EventHandler<int> TrackChanged;
        public Task<bool> StartBackgroundTaskAsync()
        {
            throw new NotImplementedException();
        }

        public void PlayOrPause()
        {
            throw new NotImplementedException();
        }

        public void Play(Track track)
        {
            throw new NotImplementedException();
        }

        public void Play(Track track, List<Track> tracks)
        {
            throw new NotImplementedException();
        }

        public void Play(List<Track> tracks)
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