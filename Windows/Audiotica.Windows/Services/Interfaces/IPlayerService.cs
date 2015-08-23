using System;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Audiotica.Core.Common;
using Audiotica.Database.Models;
using Audiotica.Web.Models;

namespace Audiotica.Windows.Services.Interfaces
{
    public interface IPlayerService
    {
        bool IsBackgroundTaskRunning { get; }
        MediaPlayerState CurrentState { get; set; }
        string CurrentQueueId { get; }
        OptimizedObservableCollection<QueueTrack> PlaybackQueue { get; }
        event EventHandler<MediaPlayerState> MediaStateChanged;
        event EventHandler<string> TrackChanged;

        /// <summary>
        ///     Initialize Background Media Player Handlers and starts playback
        /// </summary>
        Task<bool> StartBackgroundTaskAsync();

        /// <summary>
        ///     Toggles between playing and pausing.
        /// </summary>
        void PlayOrPause();

        /// <summary>
        ///     Adds the specified track to the queue.
        /// </summary>
        /// <param name="track">The track.</param>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        Task<QueueTrack> AddAsync(Track track, int position = -1);

        Task<QueueTrack> AddAsync(WebSong webSong, int position = -1);
        Task<QueueTrack> AddUpNextAsync(Track track);
        Task<QueueTrack> AddUpNextAsync(WebSong webSong);

        /// <summary>
        ///     Plays the specified queue.
        /// </summary>
        /// <param name="queue">The queue.</param>
        void Play(QueueTrack queue);

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