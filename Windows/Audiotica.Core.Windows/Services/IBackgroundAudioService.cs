using System;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Audiotica.Database.Models;

namespace Audiotica.Core.Windows.Services
{
    public interface IBackgroundAudioService
    {
        bool IsBackgroundTaskRunning { get; }
        MediaPlayerState CurrentState { get; set; }
        int CurrentQueueId { get; }
        event EventHandler<MediaPlayerState> MediaStateChanged;
        event EventHandler<QueueItem> QueueChanged;

        /// <summary>
        ///     Initialize Background Media Player Handlers and starts playback
        /// </summary>
        Task<bool> StartBackgroundTaskAsync();

        void PlayOrPause();
        void Play(QueueItem item);

        /// <summary>
        ///     Sends message to the background task to skip to the previous track.
        /// </summary>
        void Previous();

        /// <summary>
        ///     Tells the background audio agent to skip to the next track.
        /// </summary>
        void Next();

        /// <summary>
        ///     Sends message to background informing app has resumed
        ///     Subscribe to MediaPlayer events
        /// </summary>
        void Resuming();

        /// <summary>
        ///     Send message to Background process that app is to be suspended
        ///     Stop clock and slider when suspending
        ///     Unsubscribe handlers for MediaPlayer events
        /// </summary>
        void Suspending();
    }
}