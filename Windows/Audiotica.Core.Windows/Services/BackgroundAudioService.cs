using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Core.Windows.Enums;
using Audiotica.Core.Windows.Extensions;
using Audiotica.Core.Windows.Helpers;
using Audiotica.Core.Windows.Messages;
using Audiotica.Database.Models;

namespace Audiotica.Core.Windows.Services
{
    public class BackgroundAudioService : IBackgroundAudioService
    {
        private readonly AutoResetEvent _backgroundAudioTaskStarted;
        private readonly IDispatcherUtility _dispatcherUtility;
        private readonly ISettingsUtility _settingsUtility;
        private bool _isMyBackgroundTaskRunning;

        public BackgroundAudioService(ISettingsUtility settingsUtility, IDispatcherUtility dispatcherUtility)
        {
            _settingsUtility = settingsUtility;
            _dispatcherUtility = dispatcherUtility;
            // Setup the initialization lock
            _backgroundAudioTaskStarted = new AutoResetEvent(false);
            _settingsUtility.Write(ApplicationSettingsConstants.AppState, AppState.Active);
        }

        private void UpdatePlaybackQueue()
        {
            try
            {
                var tracks = BackgroundMediaPlayer.Current.Source as MediaPlaybackList;
                if (tracks != null)
                {
                    var queueTracks = tracks.Items.Select(p => p.Source.Queue()).ToList();
                    PlaybackQueue = new OptimizedObservableCollection<QueueTrack>(queueTracks);
                }
            }
            catch (InvalidCastException)
            {
                // source is not set (odd exception tho)
            }
        }

        public bool IsBackgroundTaskRunning
        {
            get
            {
                if (_isMyBackgroundTaskRunning)
                    return true;

                var value = _settingsUtility.Read(ApplicationSettingsConstants.BackgroundTaskState,
                    BackgroundTaskState.Unknown);
                return _isMyBackgroundTaskRunning = value == BackgroundTaskState.Running;
            }
        }

        public MediaPlayerState CurrentState { get; set; }

        public string CurrentQueueId
            => _settingsUtility.Read(ApplicationSettingsConstants.QueueId, string.Empty);

        public OptimizedObservableCollection<QueueTrack> PlaybackQueue { get; private set; }
        public event EventHandler<MediaPlayerState> MediaStateChanged;
        public event EventHandler<string> TrackChanged;

        /// <summary>
        ///     Initialize Background Media Player Handlers and starts playback
        /// </summary>
        public async Task<bool> StartBackgroundTaskAsync()
        {
            AddMediaPlayerEventHandlers();

            var started = await _dispatcherUtility.RunAsync(() =>
            {
                var result = _backgroundAudioTaskStarted.WaitOne(10000);
                return result;
            });
            if (started)
                UpdatePlaybackQueue();
            return started;
        }

        public QueueTrack Add(Track track)
        {
            var queue = new QueueTrack(track);
            MessageHelper.SendMessageToBackground(new AddToPlaylistMessage(queue));
            PlaybackQueue.Add(queue);
            return queue;
        }

        public void SwitchTo(List<Track> tracks)
        {
            var queue = tracks.Select(p => new QueueTrack(p)).ToList();
            MessageHelper.SendMessageToBackground(new UpdatePlaylistMessage(queue));
            PlaybackQueue = new OptimizedObservableCollection<QueueTrack>(queue);
        }

        public void Play(QueueTrack queue)
        {
            // Switch to the selected track
            MessageHelper.SendMessageToBackground(new TrackChangedMessage(queue.Id));

            if (MediaPlayerState.Paused == BackgroundMediaPlayer.Current.CurrentState)
            {
                BackgroundMediaPlayer.Current.Play();
            }
        }

        /// <summary>
        ///     Sends message to the background task to skip to the previous track.
        /// </summary>
        public void Previous()
        {
            MessageHelper.SendMessageToBackground(new SkipPreviousMessage());
        }

        /// <summary>
        ///     Tells the background audio agent to skip to the next track.
        /// </summary>
        public void Next()
        {
            MessageHelper.SendMessageToBackground(new SkipNextMessage());
        }

        /// <summary>
        ///     Sends message to background informing app has resumed
        ///     Subscribe to MediaPlayer events
        /// </summary>
        public void Resuming()
        {
            _settingsUtility.Write(ApplicationSettingsConstants.AppState, AppState.Active);

            // Verify the task is running
            if (IsBackgroundTaskRunning)
            {
                // If yes, it's safe to reconnect to media play handlers
                AddMediaPlayerEventHandlers();

                // Send message to background task that app is resumed so it can start sending notifications again
                MessageHelper.SendMessageToBackground(new AppResumedMessage());

                MediaPlayer_CurrentStateChanged(BackgroundMediaPlayer.Current, null);
            }
        }

        /// <summary>
        ///     Send message to Background process that app is to be suspended
        ///     Stop clock and slider when suspending
        ///     Unsubscribe handlers for MediaPlayer events
        /// </summary>
        public void Suspending()
        {
            // Only if the background task is already running would we do these, otherwise
            // it would trigger starting up the background task when trying to suspend.
            if (IsBackgroundTaskRunning)
            {
                // Stop handling player events immediately
                RemoveMediaPlayerEventHandlers();

                // Tell the background task the foreground is suspended
                MessageHelper.SendMessageToBackground(new AppSuspendedMessage());
            }

            // Persist that the foreground app is suspended
            _settingsUtility.Write(ApplicationSettingsConstants.AppState, AppState.Suspended);
        }

        /// <summary>
        ///     If the task is already running, it will just play/pause MediaPlayer Instance
        ///     Otherwise, initializes MediaPlayer Handlers and starts playback
        ///     track or to pause if we're already playing.
        /// </summary>
        public async void PlayOrPause()
        {
            Debug.WriteLine("Play button pressed from App");
            if (IsBackgroundTaskRunning)
            {
                switch (BackgroundMediaPlayer.Current.CurrentState)
                {
                    case MediaPlayerState.Playing:
                        BackgroundMediaPlayer.Current.Pause();
                        break;
                    case MediaPlayerState.Paused:
                        BackgroundMediaPlayer.Current.Play();
                        break;
                    case MediaPlayerState.Closed:
                        await StartBackgroundTaskAsync();
                        break;
                }
            }
            else
            {
                await StartBackgroundTaskAsync();
            }
        }

        /// <summary>
        ///     Subscribes to MediaPlayer events
        /// </summary>
        private void AddMediaPlayerEventHandlers()
        {
            BackgroundMediaPlayer.Current.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        /// <summary>
        ///     Unsubscribes to MediaPlayer events. Should run only on suspend
        /// </summary>
        private void RemoveMediaPlayerEventHandlers()
        {
            BackgroundMediaPlayer.Current.CurrentStateChanged -= MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground -= BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        #region Background MediaPlayer Event handlers

        /// <summary>
        ///     MediaPlayer state changed event handlers.
        ///     Note that we can subscribe to events even if Media Player is playing media in background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            CurrentState = sender.CurrentState;
            await _dispatcherUtility.RunAsync(() => { MediaStateChanged?.Invoke(sender, CurrentState); });
        }

        /// <summary>
        ///     This event is raised when a message is recieved from BackgroundAudioTask
        /// </summary>
        private async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender,
            MediaPlayerDataReceivedEventArgs e)
        {
            var message = MessageHelper.ParseMessage(e.Data);
            if (message is TrackChangedMessage)
            {
                var trackChangedMessage = message as TrackChangedMessage;
                // When foreground app is active change track based on background message
                await
                    _dispatcherUtility.RunAsync(
                        () => { TrackChanged?.Invoke(sender, trackChangedMessage.QueueId); });
                return;
            }

            if (message is BackgroundAudioTaskStartedMessage)
            {
                // StartBackgroundAudioTask is waiting for this signal to know when the task is up and running
                // and ready to receive messages
                Debug.WriteLine("BackgroundAudioTask started");
                _backgroundAudioTaskStarted.Set();
            }
        }

        #endregion
    }
}