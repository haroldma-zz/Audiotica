#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Audiotica.Core;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;

#endregion

namespace Audiotica
{
    public class AudioPlayerManager : ObservableObject
    {
        private readonly ICollectionService _service;

        public AudioPlayerManager(ICollectionService service)
        {
            _service = service; 
            _sererInitialized = new AutoResetEvent(false);
            AppSettingsHelper.Write(PlayerConstants.AppState, PlayerConstants.ForegroundAppActive);
            _nextRelayCommand = new RelayCommand(NextSong);
            _prevRelayCommand = new RelayCommand(PrevSong);
            _playPauseRelayCommand = new RelayCommand(PlayPauseToggle);
        }

        public void Initialize()
        {
            StartBackgroundAudioTask(false);
        }

        #region Private Fields and Properties

        private readonly AutoResetEvent _sererInitialized;
        private bool _isPlayerRunning;
        private readonly RelayCommand _nextRelayCommand;
        private readonly RelayCommand _prevRelayCommand;
        private readonly RelayCommand _playPauseRelayCommand;
        private bool _handlersAttached = false;
        private bool _loading;

        private bool IsPlayerRunning
        {
            get
            {
                if (_isPlayerRunning) return true;

                var value = AppSettingsHelper.Read(PlayerConstants.BackgroundTaskState);
                if (value == null)
                {
                    return false;
                }
                _isPlayerRunning = value.Equals(PlayerConstants.BackgroundTaskRunning);
                return _isPlayerRunning;
            }
        }

        #endregion

        #region Background MediaPlayer Event handlers

        private void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
// ReSharper disable ExplicitCallerInfoArgument
            DispatcherHelper.RunAsync(() =>
            {
                RaisePropertyChanged("CurrentPlayPauseIcon");
                switch (sender.CurrentState)
                {
                    case MediaPlayerState.Playing:
                        IsLoading = false;
                        RaisePropertyChanged("CurrentSong");
                        break;
                    default:
                        IsLoading = false;
                        break;
                    case MediaPlayerState.Opening:
                    case MediaPlayerState.Buffering:
                        IsLoading = true;
                        break;
                }
            });
// ReSharper restore ExplicitCallerInfoArgument
        }

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
                        _isPlayerRunning = true;
                        break;
                }
            }
        }

        #endregion

        #region Media Playback Helper methods

        public async Task ShutdownPlayerAsync()
        {
            BackgroundMediaPlayer.Shutdown();
            RemoveMediaPlayerEventHandlers();

            _isPlayerRunning = false;
            AppSettingsHelper.Write(PlayerConstants.CurrentTrack,null);

            await Task.Delay(1000);

// ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged("CurrentSong");
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
            _handlersAttached = true;
            BackgroundMediaPlayer.Current.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        /// <summary>
        ///     Initialize Background Media Player Handlers and starts playback
        /// </summary>
        private void StartBackgroundAudioTask(bool play = true)
        {
            AddMediaPlayerEventHandlers();

            if (!play)return;
            Task.Delay(2000);
            var message = new ValueSet { { PlayerConstants.StartPlayback, null } };
            BackgroundMediaPlayer.SendMessageToBackground(message);
//            var backgroundtaskinitializationresult = DispatcherHelper.RunAsync(
//                () =>
//                {
//                    IsLoading = true;
//                    _sererInitialized.WaitOne(2000);
//                    //assuming that the task starts always
//                    var message = new ValueSet {{PlayerConstants.StartPlayback, null}};
//                    BackgroundMediaPlayer.SendMessageToBackground(message);
//                }
//                );
//            backgroundtaskinitializationresult.Completed = BackgroundTaskInitializationCompleted;
        }

        #endregion

        #region media control

        public async void PlayPauseToggle()
        {
            await Task.Factory.StartNew(() =>
            {
                if (IsPlayerRunning)
                {
                    if (MediaPlayerState.Playing == BackgroundMediaPlayer.Current.CurrentState)
                    {
                        BackgroundMediaPlayer.Current.Pause();
                    }
                    else if (MediaPlayerState.Paused == BackgroundMediaPlayer.Current.CurrentState)
                    {
                        BackgroundMediaPlayer.Current.Play();
                    }
                    else if (MediaPlayerState.Closed == BackgroundMediaPlayer.Current.CurrentState
                        && !_handlersAttached)
                    {
                        StartBackgroundAudioTask();
                    }
                }
                else
                {
                    StartBackgroundAudioTask();
                }
            });
        }

        public void PlaySong(long id)
        {
            AppSettingsHelper.Write(PlayerConstants.CurrentTrack, id);

            if (IsPlayerRunning)
            {
                var message = new ValueSet {{PlayerConstants.StartPlayback, null}};
                BackgroundMediaPlayer.SendMessageToBackground(message);
            }
            else
                StartBackgroundAudioTask();
        }

        private void PrevSong()
        {
            var value = new ValueSet {{PlayerConstants.SkipPrevious, ""}};
            BackgroundMediaPlayer.SendMessageToBackground(value);
        }

        private void NextSong()
        {
            var value = new ValueSet {{PlayerConstants.SkipNext, ""}};
            BackgroundMediaPlayer.SendMessageToBackground(value);
        }

        #endregion

        public Song CurrentSong
        {
            get
            {
                if (!IsPlayerRunning) return null;

                var id = AppSettingsHelper.Read<long>(PlayerConstants.CurrentTrack);
                return _service.Songs.FirstOrDefault(p => p.Id == id);
            }
        }

        public IconElement CurrentPlayPauseIcon
        {
            get
            {
                return new SymbolIcon
                {
                    Symbol =
                        (IsPlayerRunning && BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing)
                            ? Symbol.Pause
                            : Symbol.Play
                };
            }
        }

        public bool IsLoading { get { return _loading; } set { Set(ref _loading, value); } }

        public RelayCommand NextRelayCommand
        {
            get { return _nextRelayCommand; }
        }

        public RelayCommand PrevRelayCommand
        {
            get { return _prevRelayCommand; }
        }

        public RelayCommand PlayPauseRelayCommand
        {
            get { return _playPauseRelayCommand; }
        }
    }
}