using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Audiotica.Core.Common;
using Audiotica.Core.Exceptions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Core.Windows.Enums;
using Audiotica.Core.Windows.Extensions;
using Audiotica.Core.Windows.Helpers;
using Audiotica.Core.Windows.Messages;
using Audiotica.Database.Models;
using Audiotica.Web.MatchEngine.Interfaces.Providers;
using Audiotica.Web.Models;
using Audiotica.Windows.Common;
using Audiotica.Windows.Services.Interfaces;

namespace Audiotica.Windows.Services.RunTime
{
    public class PlayerService : IPlayerService
    {
        private readonly AutoResetEvent _backgroundAudioTaskStarted;
        private readonly IDispatcherUtility _dispatcherUtility;
        private readonly IMatchEngineService _matchEngineService;
        private readonly ISettingsUtility _settingsUtility;
        private readonly IConverter<WebSong, Track> _webSongConverter;
        private bool _isMyBackgroundTaskRunning;

        public PlayerService(ISettingsUtility settingsUtility, IDispatcherUtility dispatcherUtility,
            IMatchEngineService matchEngineService, IConverter<WebSong, Track> webSongConverter)
        {
            _settingsUtility = settingsUtility;
            _dispatcherUtility = dispatcherUtility;
            _matchEngineService = matchEngineService;
            _webSongConverter = webSongConverter;
            // Setup the initialization lock
            _backgroundAudioTaskStarted = new AutoResetEvent(false);
            _settingsUtility.Write(ApplicationSettingsConstants.AppState, AppState.Active);
            PlaybackQueue = new OptimizedObservableCollection<QueueTrack>();
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
        public bool StartBackgroundTask()
        {
            AddMediaPlayerEventHandlers();

            var started = _backgroundAudioTaskStarted.WaitOne(250);
            if (started)
                UpdatePlaybackQueue();
            return started;
        }

        public async Task<QueueTrack> AddAsync(Track track, int position = -1)
        {
            await PrepareTrackAsync(track);
            return await InternalAddAsync(track, position);
        }

        public async Task AddAsync(IEnumerable<Track> tracks, int position = -1)
        {
            var arr = tracks.ToArray();
            foreach (var track in arr)
                await PrepareTrackAsync(track);
            Add(arr, position);
        }

        public async Task<QueueTrack> AddAsync(WebSong webSong, int position = -1)
        {
            var track = await ConvertToTrackAsync(webSong);
            return await AddAsync(track, position);
        }

        public Task<QueueTrack> AddUpNextAsync(Track track)
        {
            var currentPosition = PlaybackQueue.IndexOf(PlaybackQueue.FirstOrDefault(p => p.Id == CurrentQueueId));
            return AddAsync(track, currentPosition + 1);
        }

        public async Task AddUpNextAsync(IEnumerable<Track> tracks)
        {
            var currentPosition = PlaybackQueue.IndexOf(PlaybackQueue.FirstOrDefault(p => p.Id == CurrentQueueId));
            await AddAsync(tracks, currentPosition + 1);
        }

        public async Task<QueueTrack> AddUpNextAsync(WebSong webSong)
        {
            var track = await ConvertToTrackAsync(webSong);
            return await AddUpNextAsync(track);
        }

        public async Task NewQueueAsync(IEnumerable<Track> tracks)
        {
            var arr = tracks.ToArray();
            foreach (var track in arr)
                await PrepareTrackAsync(track);
            var newQueue = arr.Select(track => new QueueTrack(track)).ToList();
            PlaybackQueue.SwitchTo(newQueue);
            MessageHelper.SendMessageToBackground(new UpdatePlaylistMessage(newQueue));

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
        public void PlayOrPause()
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
                        StartBackgroundTask();
                        break;
                }
            }
            else
            {
                StartBackgroundTask();
            }
        }

        private async Task<Track> ConvertToTrackAsync(WebSong webSong)
        {
            var track = webSong.PreviousConversion as Track;
            if (track == null)
                using (var blocker = new UiBlocker())
                {
                    blocker.UpdateProgress("Getting data...");
                    track = await _webSongConverter.ConvertAsync(webSong);
                }
            return track;
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

        private async Task<QueueTrack> InternalAddAsync(Track track, int position = -1)
        {
            var queue = new QueueTrack(track);
            if (position > -1 && position < PlaybackQueue.Count)
                PlaybackQueue.Insert(position, queue);
            else
                PlaybackQueue.Add(queue);
            MessageHelper.SendMessageToBackground(new AddToPlaylistMessage(queue, position));
            await Task.Delay(25);
            return queue;
        }

        private void Add(IEnumerable<Track> tracks, int position = -1)
        {
            var queue = tracks.Select(track => new QueueTrack(track)).ToList();
            if (position > -1 && position < PlaybackQueue.Count)
                foreach (var item in queue)
                {
                    PlaybackQueue.Insert(position++, item);
                }
            else
                PlaybackQueue.AddRange(queue);
            MessageHelper.SendMessageToBackground(new AddToPlaylistMessage(queue, position));
        }

        private async Task PrepareTrackAsync(Track track)
        {
            switch (track.Status)
            {
                case TrackStatus.Matching:
                    throw new AppException("Track is still matching.");
                case TrackStatus.NoMatch:
                    throw new AppException("No match found for track, try manual matching it.");
                case TrackStatus.NotAvailable:
                    throw new AppException("The audio file is not available.");
            }

            if (track.AudioWebUri == null)
                using (var blocker = new UiBlocker())
                {
                    blocker.UpdateProgress("Matching...");
                    var uri = await _matchEngineService.GetLinkAsync(track.Title, track.DisplayArtist);

                    if (uri == null)
                    {
                        throw new AppException("Problem matching the song, try saving and manual matching it.");
                    }
                    track.AudioWebUri = uri.ToString();
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
                await _dispatcherUtility.RunAsync(
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