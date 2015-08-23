using System;
using System.Diagnostics;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.Media.Playback;
using Audiotica.Core.Windows.Enums;
using Audiotica.Core.Windows.Helpers;
using Audiotica.Core.Windows.Messages;
using Audiotica.Core.Windows.Utilities;

namespace Audiotica.Windows.Player
{
    public sealed class BackgroundAudioTask : IBackgroundTask
    {
        // Used to keep task alive
        private BackgroundTaskDeferral _deferral;
        private ForegroundMessenger _foregroundMessenger;
        private PlayerWrapper _playerWrapper;
        private SettingsUtility _settingsUtility;
        private SmtcWrapper _smtcWrapper;
        internal static ManualResetEvent TaskStarted { get; } = new ManualResetEvent(false);

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _settingsUtility = new SettingsUtility();

            _foregroundMessenger = new ForegroundMessenger();
            _smtcWrapper = new SmtcWrapper(BackgroundMediaPlayer.Current.SystemMediaTransportControls);
            _playerWrapper = new PlayerWrapper(_smtcWrapper, _foregroundMessenger, _settingsUtility);

            _settingsUtility.Write(ApplicationSettingsConstants.BackgroundTaskState,
                BackgroundTaskState.Running);

            // Send information to foreground that background task has been started if app is active
            if (_playerWrapper.ForegroundAppState != AppState.Suspended)
                MessageHelper.SendMessageToForeground(new BackgroundAudioTaskStartedMessage());

            // This must be retrieved prior to subscribing to events below which use it
            _deferral = taskInstance.GetDeferral();

            // Mark the background task as started to unblock SMTC Play operation (see related WaitOne on this signal)
            TaskStarted.Set();

            // Associate a cancellation and completed handlers with the background task.
            taskInstance.Task.Completed += TaskCompleted;
            // event may raise immediately before continung thread excecution so must be at the end
            taskInstance.Canceled += OnCanceled;
        }

        private void TaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Debug.WriteLine("MyBackgroundAudioTask " + sender.TaskId + " Completed...");
            _deferral.Complete();
        }

        /// <summary>
        ///     Handles background task cancellation. Task cancellation happens due to:
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
                // immediately set not running
                TaskStarted.Reset();

                // Dispose
                _playerWrapper.Dispose();
                _smtcWrapper.Dispose();
                _foregroundMessenger.Dispose();

                // shutdown media pipeline
                BackgroundMediaPlayer.Shutdown();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            _settingsUtility.Write(ApplicationSettingsConstants.BackgroundTaskState,
                BackgroundTaskState.Canceled);

            _deferral.Complete(); // signals task completion. 
            Debug.WriteLine("MyBackgroundAudioTask Cancel complete...");
        }
    }
}