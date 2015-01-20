#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Playback;
using Audiotica.Core;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.RunTime;

#endregion

namespace Audiotica.WindowsPhone.Player
{
    /// <summary>
    ///     Enum to identify foreground app state
    /// </summary>
    internal enum ForegroundAppStatus
    {
        Active,
        Suspended,
        Unknown
    }

    public sealed class AudioTask : IBackgroundTask
    {
        #region Private fields, properties

        private readonly AutoResetEvent _backgroundTaskStarted = new AutoResetEvent(false);
        private bool _backgroundtaskrunning;
        private BackgroundTaskDeferral _deferral; // Used to keep task alive

        private ForegroundAppStatus _foregroundAppState
        {
            get
            {
                var value = AppSettingsHelper.Read(PlayerConstants.AppState);
                if (value == null)
                    return ForegroundAppStatus.Unknown;
                else
                    return (ForegroundAppStatus)Enum.Parse(typeof(ForegroundAppStatus), value);
            }
        }
        private QueueManager _queueManager;
        private SystemMediaTransportControls _systemmediatransportcontrol;

        /// <summary>
        ///     Property to hold current playlist
        /// </summary>
        private QueueManager QueueManager
        {
            get
            {
                if (_queueManager != null) return _queueManager;

                
                _queueManager = new QueueManager();
                return _queueManager;
            }
        }

        #endregion

        #region IBackgroundTask and IBackgroundTaskInstance Interface Members and handlers

        /// <summary>
        ///     The Run method is the entry point of a background task.
        /// </summary>
        /// <param name="taskInstance"></param>
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Background Audio Task " + taskInstance.Task.Name + " starting...");
            // InitializeAsync SMTC object to talk with UVC. 
            //Note that, this is intended to run after app is paused and 
            //hence all the logic must be written to run in background process
            _systemmediatransportcontrol = SystemMediaTransportControls.GetForCurrentView();
            _systemmediatransportcontrol.ButtonPressed += systemmediatransportcontrol_ButtonPressed;
            _systemmediatransportcontrol.PropertyChanged += systemmediatransportcontrol_PropertyChanged;
            _systemmediatransportcontrol.IsEnabled = true;
            _systemmediatransportcontrol.IsPauseEnabled = true;
            _systemmediatransportcontrol.IsPlayEnabled = true;
            _systemmediatransportcontrol.IsNextEnabled = true;
            _systemmediatransportcontrol.IsPreviousEnabled = true;

            // Associate a cancellation and completed handlers with the background task.
            taskInstance.Canceled += OnCanceled;
            taskInstance.Task.Completed += Taskcompleted;

            //Add handlers for MediaPlayer
            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;

            //Add handlers for playlist trackchanged
            QueueManager.TrackChanged += playList_TrackChanged;

            //InitializeAsync message channel 
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;

            //Send information to foreground that background task has been started if app is active
            if (_foregroundAppState != ForegroundAppStatus.Suspended)
            {
                var message = new ValueSet {{PlayerConstants.BackgroundTaskStarted, ""}};
                BackgroundMediaPlayer.SendMessageToForeground(message);
            }
            _backgroundTaskStarted.Set();
            _backgroundtaskrunning = true;

            AppSettingsHelper.Write(PlayerConstants.BackgroundTaskState,
                PlayerConstants.BackgroundTaskRunning);
            _deferral = taskInstance.GetDeferral();
        }

        /// <summary>
        ///     Indicate that the background task is completed.
        /// </summary>
        private void Taskcompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Debug.WriteLine("Audio Task " + sender.TaskId + " Completed...");
            _deferral.Complete();
        }

        /// <summary>
        ///     Handles background task cancellation. Task cancellation happens due to :
        ///     1. Another Media app comes into foreground and starts playing music
        ///     2. Resource pressure. Your task is consuming more CPU and memory than allowed.
        ///     In either case, save state so that if foreground app resumes it can know where to start.
        /// </summary>
        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            // You get some time here to save your state before process and resources are reclaimed
            Debug.WriteLine("MyBackgroundAudioTask " + sender.Task.TaskId + " Cancel Requested...");
            try
            {
                if (_queueManager != null)
                {
                    QueueManager.TrackChanged -= playList_TrackChanged;
                    _queueManager = null;
                }

                AppSettingsHelper.Write(PlayerConstants.BackgroundTaskState,
                    PlayerConstants.BackgroundTaskCancelled);

                _backgroundtaskrunning = false;
                //unsubscribe event handlers
                _systemmediatransportcontrol.ButtonPressed -= systemmediatransportcontrol_ButtonPressed;
                _systemmediatransportcontrol.PropertyChanged -= systemmediatransportcontrol_PropertyChanged;

                BackgroundMediaPlayer.Shutdown(); // shutdown media pipeline
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            if (_deferral != null)
            {
                _deferral.Complete(); // signals task completion. 
                Debug.WriteLine("AudioPlayer Cancel complete...");
            }
        }

        #endregion

        #region SysteMediaTransportControls related functions and handlers

        /// <summary>
        ///     Update UVC using SystemMediaTransPortControl apis
        /// </summary>
        private void UpdateUvcOnNewTrack()
        {
            _systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Playing;
            _systemmediatransportcontrol.DisplayUpdater.Type = MediaPlaybackType.Music;
            _systemmediatransportcontrol.DisplayUpdater.MusicProperties.Title = QueueManager.CurrentTrack.Song.Name;
            _systemmediatransportcontrol.DisplayUpdater.MusicProperties.Artist =
                QueueManager.CurrentTrack.Song.Artist.Name;
            _systemmediatransportcontrol.DisplayUpdater.Update();
        }

        /// <summary>
        ///     Fires when any SystemMediaTransportControl property is changed by system or user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void systemmediatransportcontrol_PropertyChanged(SystemMediaTransportControls sender,
            SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            //TODO: If soundlevel turns to muted, app can choose to pause the music
        }

        /// <summary>
        ///     This function controls the button events from UVC.
        ///     This code if not run in background process, will not be able to handle button pressed events when app is suspended.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void systemmediatransportcontrol_ButtonPressed(SystemMediaTransportControls sender,
            SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    Debug.WriteLine("UVC play button pressed");
                    // If music is in paused state, for a period of more than 5 minutes, 
                    //app will get task cancellation and it cannot run code. 
                    //However, user can still play music by pressing play via UVC unless a new app comes in clears UVC.
                    //When this happens, the task gets re-initialized and that is asynchronous and hence the wait
                    if (!_backgroundtaskrunning)
                    {
                        var result = _backgroundTaskStarted.WaitOne(2000);
                        if (!result)
                            throw new Exception("Background Task didnt initialize in time");
                        StartPlayback();
                    }
                    else
                    {
                        BackgroundMediaPlayer.Current.Play();
                    }
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    Debug.WriteLine("UVC pause button pressed");
                    try
                    {
                        BackgroundMediaPlayer.Current.Pause();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                    break;
                case SystemMediaTransportControlsButton.Next:
                    Debug.WriteLine("UVC next button pressed");
                    SkipToNext();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    Debug.WriteLine("UVC previous button pressed");
                    SkipToPrevious();
                    break;
            }
        }

        #endregion

        #region Playlist management functions and handlers

        /// <summary>
        ///     Start playlist and change UVC state
        /// </summary>
        private void StartPlayback()
        {
            try
            {
                QueueManager.StartTrack(QueueManager.GetCurrentQueueSong());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        ///     Fires when playlist changes to a new track
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void playList_TrackChanged(QueueManager sender, object args)
        {
            UpdateUvcOnNewTrack();
            AppSettingsHelper.Write(PlayerConstants.CurrentTrack, sender.CurrentTrack.Id);

            if (_foregroundAppState != ForegroundAppStatus.Active) return;

            //Message channel that can be used to send messages to foreground
            var message = new ValueSet {{PlayerConstants.Trackchanged, sender.CurrentTrack.Id}};
            BackgroundMediaPlayer.SendMessageToForeground(message);
        }

        /// <summary>
        ///     Skip track and update UVC via SMTC
        /// </summary>
        private void SkipToPrevious()
        {
            _systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Changing;
            QueueManager.SkipToPrevious();
        }

        /// <summary>
        ///     Skip track and update UVC via SMTC
        /// </summary>
        private void SkipToNext()
        {
            _systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Changing;
            QueueManager.SkipToNext();
        }

        #endregion

        #region Background Media Player Handlers

        private void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (sender.CurrentState == MediaPlayerState.Playing)
            {
                _systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Playing;
            }
            else if (sender.CurrentState == MediaPlayerState.Paused)
            {
                _systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Paused;
            }
        }


        /// <summary>
        ///     Fires when a message is recieved from the foreground app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender,
            MediaPlayerDataReceivedEventArgs e)
        {
            foreach (string key in e.Data.Keys)
            {
                switch (key.ToLower())
                {
                    case PlayerConstants.AppSuspended:
                        Debug.WriteLine("App suspending");
                        // App is suspended, you can save your task state at this point
                        AppSettingsHelper.Write(PlayerConstants.CurrentTrack, QueueManager.CurrentTrack.Id);
                        break;
                    case PlayerConstants.AppResumed:
                        Debug.WriteLine("App resuming"); // App is resumed, now subscribe to message channel
                        break;
                    case PlayerConstants.StartPlayback:
                        //Foreground App process has signalled that it is ready for playback
                        Debug.WriteLine("Starting Playback");
                        StartPlayback();
                        break;
                    case PlayerConstants.SkipNext: // User has chosen to skip track from app context.
                        Debug.WriteLine("Skipping to next");
                        SkipToNext();
                        break;
                    case PlayerConstants.SkipPrevious: // User has chosen to skip track from app context.
                        Debug.WriteLine("Skipping to previous");
                        SkipToPrevious();
                        break;
                }
            }
        }

        #endregion
    }
}