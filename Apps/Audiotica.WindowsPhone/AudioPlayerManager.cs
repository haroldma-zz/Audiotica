using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Audiotica.Collection.Model;
using Audiotica.Core;
using Audiotica.Core.Utilities;

namespace Audiotica
{
    public class AudioPlayerManager
    {
        public AudioPlayerManager()
        {
            _sererInitialized = new AutoResetEvent(false);

            AppSettingsHelper.Write(PlayerConstants.AppState, PlayerConstants.ForegroundAppActive);

           // AddMediaPlayerEventHandlers();
        }

        #region Private Fields and Properties

        private readonly AutoResetEvent _sererInitialized;
        private bool _isMyBackgroundTaskRunning;

        /// <summary>
        ///     Gets the information about background task is running or not by reading the setting saved by background task
        /// </summary>
        private bool IsMyBackgroundTaskRunning
        {
            get
            {
                if (_isMyBackgroundTaskRunning) return true;

                var value = AppSettingsHelper.Read(PlayerConstants.BackgroundTaskState);
                if (value == null)
                {
                    return false;
                }
                _isMyBackgroundTaskRunning = ((String)value).Equals(PlayerConstants.BackgroundTaskRunning);
                return _isMyBackgroundTaskRunning;
            }
        }

        #endregion

        #region Background MediaPlayer Event handlers

        /// <summary>
        ///     MediaPlayer state changed event handlers.
        ///     Note that we can subscribe to events even if Media Player is playing media in background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            //NotifyPropertyChanged("CurrentTrack");
            switch (sender.CurrentState)
            {
                case MediaPlayerState.Playing:
                    //
                    break;
                case MediaPlayerState.Paused:
                    //fire event
                    break;
                case MediaPlayerState.Opening:

                    break;
            }
        }

        /// <summary>
        ///     This event fired when a message is recieved from Background Process
        /// </summary>
        private void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender,
            MediaPlayerDataReceivedEventArgs e)
        {
            foreach (string key in e.Data.Keys)
            {
                switch (key)
                {
                    case PlayerConstants.Trackchanged:
                        //When foreground app is active change track based on background message

                        break;
                    case PlayerConstants.BackgroundTaskStarted:
                        //Wait for Background Task to be initialized before starting playback
                        Debug.WriteLine("Background Task started");
                        _sererInitialized.Set();
                        _isMyBackgroundTaskRunning = true;
                        break;
                }
            }
        }

        #endregion

        #region Media Playback Helper methods

        public void ShutdownPlayer()
        {
            BackgroundMediaPlayer.Shutdown();
            RemoveMediaPlayerEventHandlers();
        }

        /// <summary>
        ///     Unsubscribes to MediaPlayer events. Should run only on suspend
        /// </summary>
        private void RemoveMediaPlayerEventHandlers()
        {
            BackgroundMediaPlayer.Current.CurrentStateChanged -= MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground -= BackgroundMediaPlayer_MessageReceivedFromBackground;
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
        ///     Initialize Background Media Player Handlers and starts playback
        /// </summary>
        private void StartBackgroundAudioTask()
        {
            AddMediaPlayerEventHandlers();
            var backgroundtaskinitializationresult = Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    var result = _sererInitialized.WaitOne(2000);
                    //Send message to initiate playback
                    if (result)
                    {
                        var message = new ValueSet { { PlayerConstants.StartPlayback, null } };
                        BackgroundMediaPlayer.SendMessageToBackground(message);
                    }
                    else
                    {
                        throw new Exception("Background Audio Task didn't start in expected time");
                    }
                }
                );
            backgroundtaskinitializationresult.Completed = BackgroundTaskInitializationCompleted;
        }

        private void BackgroundTaskInitializationCompleted(IAsyncAction action, AsyncStatus status)
        {
            if (status == AsyncStatus.Completed)
            {
                Debug.WriteLine("Background Audio Task initialized");
            }
            else if (status == AsyncStatus.Error)
            {
                Debug.WriteLine("Background Audio Task could not initialized due to an error ::" + action.ErrorCode);
            }
        }

        #endregion

        #region media control

        public void PlayPauseToggle()
        {
            Debug.WriteLine("Play button pressed from App");
            if (IsMyBackgroundTaskRunning)
            {
                if (MediaPlayerState.Playing == BackgroundMediaPlayer.Current.CurrentState)
                {
                    BackgroundMediaPlayer.Current.Pause();
                }
                else if (MediaPlayerState.Paused == BackgroundMediaPlayer.Current.CurrentState)
                {
                    BackgroundMediaPlayer.Current.Play();
                }
                else if (MediaPlayerState.Closed == BackgroundMediaPlayer.Current.CurrentState)
                {
                    //AppSettingsHelper.Write(PlayerConstants.NowPlaying, App.Singleton.SongManager.Songs);
                    StartBackgroundAudioTask();
                }
            }
            else
            {
                //AppSettingsHelper.Write(PlayerConstants.NowPlaying, App.Singleton.SongManager.Songs);
                StartBackgroundAudioTask();
            }
        }

        public void PlaySong(int id)
        {
            AppSettingsHelper.Write(PlayerConstants.CurrentTrack, id);

            if (_isMyBackgroundTaskRunning)
            {
                var message = new ValueSet { { PlayerConstants.StartPlayback, null } };
                BackgroundMediaPlayer.SendMessageToBackground(message);
            }
            else StartBackgroundAudioTask();
        }

        /// <summary>
        ///     Sends message to the background task to skip to the previous track.
        /// </summary>
        private void PrevSong()
        {
            var value = new ValueSet { { PlayerConstants.SkipPrevious, "" } };
            BackgroundMediaPlayer.SendMessageToBackground(value);
        }


        /// <summary>
        ///     Tells the background audio agent to skip to the next track.
        /// </summary>
        private void NextSong()
        {
            var value = new ValueSet { { PlayerConstants.SkipNext, "" } };
            BackgroundMediaPlayer.SendMessageToBackground(value);
        }

        #endregion
    }
}
