using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Core.Windows.Enums;
using Audiotica.Core.Windows.Helpers;
using Audiotica.Core.Windows.Messages;
using Audiotica.Database.Models;

namespace Audiotica.Windows.Player
{
    internal class PlayerWrapper : IDisposable
    {
        private readonly ISettingsUtility _settingsUtility;
        private ForegroundMessenger _foregroundMessenger;
        private MediaPlaybackList _mediaPlaybackList;
        private bool _playbackStartedPreviously;
        private SmtcWrapper _smtcWrapper;

        public PlayerWrapper(SmtcWrapper smtcWrapper, ForegroundMessenger foregroundMessenger,
            ISettingsUtility settingsUtility)
        {
            _smtcWrapper = smtcWrapper;
            _foregroundMessenger = foregroundMessenger;
            _settingsUtility = settingsUtility;
            _mediaPlaybackList = new MediaPlaybackList();

            SubscribeToMessenger();
            SubscribeToSmtc();

            // Add handlers for MediaPlayer
            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;

            // Read persisted state of foreground app
            ForegroundAppState = _settingsUtility.Read(ApplicationSettingsConstants.AppState, AppState.Unknown);
        }

        public AppState ForegroundAppState { get; private set; }
        public int? CurrentTrackId => _mediaPlaybackList?.CurrentItem?.Source?.Id();

        public void Dispose()
        {
            // save state
            _settingsUtility.Write(ApplicationSettingsConstants.TrackId, CurrentTrackId);
            _settingsUtility.Write(ApplicationSettingsConstants.Position, BackgroundMediaPlayer.Current.Position);
            _settingsUtility.Write(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Canceled);
            _settingsUtility.Write(ApplicationSettingsConstants.AppState, _foregroundMessenger);

            UnsubscribeFromMessenger();
            UnsubscribeFromSmtc();

            _smtcWrapper = null;
            _foregroundMessenger = null;
        }

        /// <summary>
        ///     Resumes or starts playing from state.
        /// </summary>
        public void Play()
        {
            try
            {
                // If playback was already started once we can just resume playing.
                if (!_playbackStartedPreviously)
                {
                    _playbackStartedPreviously = true;

                    // If the task was cancelled we would have saved the current track and its position. We will try playback from there.
                    var currentTrackId = _settingsUtility.Read(ApplicationSettingsConstants.TrackId, -1);
                    var currentTrackPosition = _settingsUtility.Read<TimeSpan?>(ApplicationSettingsConstants.Position,
                        null);
                    if (currentTrackId != -1)
                        InternalStartPlayer(currentTrackId, currentTrackPosition);
                    else
                        BackgroundMediaPlayer.Current.Play();
                }
                else
                    BackgroundMediaPlayer.Current.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        ///     Pauses the player.
        /// </summary>
        public void Pause()
        {
            try
            {
                BackgroundMediaPlayer.Current.Pause();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        ///     Skips to next track.
        /// </summary>
        public void SkipToNext()
        {
            _smtcWrapper.PlaybackStatus = MediaPlaybackStatus.Changing;
            _mediaPlaybackList.MoveNext();

            // TODO: Work around playlist bug that doesn't continue playing after a switch; remove later
            BackgroundMediaPlayer.Current.Play();
        }

        /// <summary>
        ///     Skips to previous track.
        /// </summary>
        public void SkipToPrev()
        {
            _smtcWrapper.PlaybackStatus = MediaPlaybackStatus.Changing;
            _mediaPlaybackList.MovePrevious();

            // TODO: Work around playlist bug that doesn't continue playing after a switch; remove later
            BackgroundMediaPlayer.Current.Play();
        }

        /// <summary>
        ///     Create a playback list from the list of songs received from the foreground app.
        /// </summary>
        /// <param name="songs"></param>
        public void CreatePlaybackList(IEnumerable<Track> songs)
        {
            // Make a new list and enable looping
            _mediaPlaybackList = new MediaPlaybackList {AutoRepeatEnabled = true};

            // Add playback items to the list
            foreach (var song in songs)
            {
                var source = MediaSource.CreateFromUri(song.AudioUri);
                source.Id(song.Id);
                source.Title(song.Title);
                source.Artists(song.Artists);
                source.AlbumTitle(song.Album);
                source.AlbumArtist(song.AlbumArtist);
                source.Artwork(song.ArtworkUri);
                _mediaPlaybackList.Items.Add(new MediaPlaybackItem(source));
            }

            // Don't auto start
            BackgroundMediaPlayer.Current.AutoPlay = false;

            // Assign the list to the player
            BackgroundMediaPlayer.Current.Source = _mediaPlaybackList;

            // Add handler for future playlist item changes
            _mediaPlaybackList.CurrentItemChanged += MediaPlaybackListOnCurrentItemChanged;
        }

        #region Internal

        private void InternalStartPlayer(int currentTrackId, TimeSpan? currentTrackPosition)
        {
            // Find the index of the item by name
            var index = _mediaPlaybackList.Items.ToList().FindIndex(item =>
                item.Source.Id() == currentTrackId);

            if (index == -1) return;

            if (currentTrackPosition == null)
            {
                // Play from start if we dont have position
                Debug.WriteLine("StartPlayback: Switching to track " + index);
                _mediaPlaybackList.MoveTo((uint) index);

                // Begin playing
                BackgroundMediaPlayer.Current.Play();
            }
            else
            {
                // Play from exact position otherwise
                TypedEventHandler<MediaPlaybackList, CurrentMediaPlaybackItemChangedEventArgs> handler =
                    null;
                handler = (list, args) =>
                {
                    if (args.NewItem == _mediaPlaybackList.Items[index])
                    {
                        // Unsubscribe because this only had to run once for this item
                        _mediaPlaybackList.CurrentItemChanged -= handler;

                        // Set position
                        Debug.WriteLine("StartPlayback: Setting Position " + currentTrackPosition);
                        BackgroundMediaPlayer.Current.Position = currentTrackPosition.Value;

                        // Begin playing
                        BackgroundMediaPlayer.Current.Play();
                    }
                };
                _mediaPlaybackList.CurrentItemChanged += handler;

                // Switch to the track which will trigger an item changed event
                Debug.WriteLine("StartPlayback: Switching to track " + index);
                _mediaPlaybackList.MoveTo((uint) index);
            }
        }

        private void MediaPlaybackListOnCurrentItemChanged(MediaPlaybackList sender,
            CurrentMediaPlaybackItemChangedEventArgs args)
        {
            // Get the new item
            var item = args.NewItem;

            // Update the system view
            _smtcWrapper.UpdateUvcOnNewTrack(item);

            // Get the current track
            var currentTrackId = -1;
            if (item != null)
                currentTrackId = item.Source.Id();

            Debug.WriteLine("PlaybackList_CurrentItemChanged: " + currentTrackId);

            // Notify foreground of change or persist for later
            if (ForegroundAppState == AppState.Active)
                MessageHelper.SendMessageToForeground(new TrackChangedMessage(currentTrackId));
            else
                _settingsUtility.Write(ApplicationSettingsConstants.TrackId,
                    currentTrackId == -1 ? default(int?) : currentTrackId);
        }

        private void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            switch (sender.CurrentState)
            {
                case MediaPlayerState.Playing:
                    _smtcWrapper.PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
                case MediaPlayerState.Paused:
                    _smtcWrapper.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
                case MediaPlayerState.Closed:
                    _smtcWrapper.PlaybackStatus = MediaPlaybackStatus.Closed;
                    break;
            }
        }

        #region Smtc

        private void SubscribeToSmtc()
        {
            _smtcWrapper.PlayPressed += SmtcWrapperOnPlayPressed;
            _smtcWrapper.PausePressed += SmtcWrapperOnPausePressed;
            _smtcWrapper.NextPressed += SmtcWrapperOnNextPressed;
            _smtcWrapper.PrevPressed += SmtcWrapperOnPrevPressed;
        }

        private void UnsubscribeFromSmtc()
        {
            _smtcWrapper.PlayPressed -= SmtcWrapperOnPlayPressed;
            _smtcWrapper.PausePressed -= SmtcWrapperOnPausePressed;
            _smtcWrapper.NextPressed -= SmtcWrapperOnNextPressed;
            _smtcWrapper.PrevPressed -= SmtcWrapperOnPrevPressed;
        }

        private void SmtcWrapperOnPlayPressed(object sender, EventArgs eventArgs)
        {
            // When the background task has been suspended and the SMTC
            // starts it again asynchronously, some time is needed to let
            // the task startup process in Run() complete.

            // Wait for task to start. 
            // Once started, this stays signaled until shutdown so it won't wait
            // again unless it needs to.
            var result = BackgroundAudioTask.TaskStarted.WaitOne(5000);
            if (!result)
                throw new Exception("Background Task didnt initialize in time");

            Play();
        }

        private void SmtcWrapperOnPausePressed(object sender, EventArgs eventArgs)
        {
            Pause();
        }

        private void SmtcWrapperOnPrevPressed(object sender, EventArgs eventArgs)
        {
            SkipToPrev();
        }

        private void SmtcWrapperOnNextPressed(object sender, EventArgs eventArgs)
        {
            SkipToNext();
        }

        #endregion

        #region Messenger

        private void SubscribeToMessenger()
        {
            _foregroundMessenger.SkipToNext += ForegroundMessengerOnSkipToNext;
            _foregroundMessenger.SkipToPrev += ForegroundMessengerOnSkipToPrev;
            _foregroundMessenger.StartPlayback += ForegroundMessengerOnStartPlayback;
            _foregroundMessenger.TrackChanged += ForegroundMessengerOnTrackChanged;
            _foregroundMessenger.UpdatePlaylist += ForegroundMessengerOnUpdatePlaylist;
            _foregroundMessenger.AppSuspended += ForegroundMessengerOnAppSuspended;
            _foregroundMessenger.AppResumed += ForegroundMessengerOnAppResumed;
        }

        private void ForegroundMessengerOnAppResumed(object sender, EventArgs eventArgs)
        {
            Debug.WriteLine("App resuming");
            ForegroundAppState = AppState.Active;
        }

        private void ForegroundMessengerOnAppSuspended(object sender, EventArgs eventArgs)
        {
            // App is suspended, you can save your task state at this point
            Debug.WriteLine("App suspending");
            ForegroundAppState = AppState.Suspended;
            _settingsUtility.Write(ApplicationSettingsConstants.TrackId, CurrentTrackId);
        }

        private void UnsubscribeFromMessenger()
        {
            _foregroundMessenger.SkipToNext -= ForegroundMessengerOnSkipToNext;
            _foregroundMessenger.SkipToPrev -= ForegroundMessengerOnSkipToPrev;
            _foregroundMessenger.StartPlayback -= ForegroundMessengerOnStartPlayback;
            _foregroundMessenger.TrackChanged -= ForegroundMessengerOnTrackChanged;
            _foregroundMessenger.UpdatePlaylist -= ForegroundMessengerOnUpdatePlaylist;
        }

        private void ForegroundMessengerOnUpdatePlaylist(object sender, List<Track> tracks)
        {
            CreatePlaybackList(tracks);
        }

        private void ForegroundMessengerOnTrackChanged(object sender, int trackId)
        {
            var index = _mediaPlaybackList.Items.ToList().FindIndex(i => i.Source.Id() == trackId);
            Debug.WriteLine("Skipping to track " + index);
            _smtcWrapper.PlaybackStatus = MediaPlaybackStatus.Changing;
            _mediaPlaybackList.MoveTo((uint) index);

            // TODO: Work around playlist bug that doesn't continue playing after a switch; remove later
            BackgroundMediaPlayer.Current.Play();
        }

        private void ForegroundMessengerOnStartPlayback(object sender, EventArgs eventArgs)
        {
            Play();
        }

        private void ForegroundMessengerOnSkipToPrev(object sender, EventArgs eventArgs)
        {
            SkipToPrev();
        }

        private void ForegroundMessengerOnSkipToNext(object sender, EventArgs eventArgs)
        {
            SkipToNext();
        }

        #endregion

        #endregion
    }
}