using System;
using System.Linq;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Audiotica.Core;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Xamarin;

namespace Audiotica
{
    public class PlayerViewModel : ViewModelBase
    {
        private readonly IAppSettingsHelper _appSettingsHelper;
        private readonly AudioPlayerHelper _helper;
        private readonly RelayCommand _nextRelayCommand;
        private readonly RelayCommand _playPauseRelayCommand;
        private readonly RelayCommand _prevRelayCommand;
        private readonly ICollectionService _service;
        private readonly DispatcherTimer _timer;
        private QueueSong _currentQueue;
        private TimeSpan _duration;
        private bool _isLoading;
        private bool _isPlayerActive;
        private double _npbHeight = double.NaN;
        private double _npHeight;
        private Symbol _playPauseIcon;
        private TimeSpan _position;

        public PlayerViewModel(
            AudioPlayerHelper helper,
            ICollectionService service,
            IAppSettingsHelper appSettingsHelper)
        {
            _helper = helper;
            _service = service;
            _appSettingsHelper = appSettingsHelper;

            if (!IsInDesignMode)
            {
                helper.TrackChanged += HelperOnTrackChanged;
                helper.PlaybackStateChanged += HelperOnPlaybackStateChanged;
                helper.Shutdown += HelperOnShutdown;

                _nextRelayCommand = new RelayCommand(NextSong);
                _prevRelayCommand = new RelayCommand(PrevSong);
                _playPauseRelayCommand = new RelayCommand(PlayPauseToggle);

                _timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
                _timer.Tick += TimerOnTick;
            }
            else
            {
                CurrentQueue = service.PlaybackQueue.FirstOrDefault();
                PlayPauseIcon = Symbol.Play;
            }
        }

        public bool IsRepeat
        {
            get { return _appSettingsHelper.Read<bool>("Repeat"); }

            set
            {
                _appSettingsHelper.Write("Repeat", value);
                RaisePropertyChanged();
            }
        }

        public bool IsShuffle
        {
            get { return _appSettingsHelper.Read<bool>("Shuffle"); }

            set
            {
                _appSettingsHelper.Write("Shuffle", value);
                _service.ShuffleModeChanged();
                RaisePropertyChanged();
                AudioPlayerHelper.OnShuffleChanged();
                Insights.Track("Shuffle", "Enabled", value ? "True" : "False");
            }
        }

        public bool IsPlayerActive
        {
            get { return _isPlayerActive; }

            set { Set(ref _isPlayerActive, value); }
        }

        public TimeSpan Duration
        {
            get { return _duration; }

            set { Set(ref _duration, value); }
        }

        public TimeSpan Position
        {
            get { return _position; }

            set { Set(ref _position, value); }
        }

        public QueueSong CurrentQueue
        {
            get { return _currentQueue; }

            set { Set(ref _currentQueue, value); }
        }

        public Symbol PlayPauseIcon
        {
            get { return _playPauseIcon; }

            set { Set(ref _playPauseIcon, value); }
        }

        public bool IsLoading
        {
            get { return _isLoading; }

            set { Set(ref _isLoading, value); }
        }

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

        public double NowPlayingHeight
        {
            get { return _npHeight; }

            set { Set(ref _npHeight, value); }
        }

        public double NowPlayingBarHeight
        {
            get { return _npbHeight; }

            set { Set(ref _npbHeight, value); }
        }

        public ICollectionService CollectionService
        {
            get { return _service; }
        }

        public AudioPlayerHelper AudioPlayerHelper
        {
            get { return _helper; }
        }

        private void HelperOnPlaybackStateChanged(object sender, PlaybackStateEventArgs playbackStateEventArgs)
        {
            IsLoading = false;
            switch (playbackStateEventArgs.State)
            {
                default:
                    PlayPauseIcon = Symbol.Play;
                    _timer.Stop();
                    break;
                case MediaPlayerState.Playing:
                    _timer.Start();
                    PlayPauseIcon = Symbol.Pause;
                    break;
                case MediaPlayerState.Buffering:
                case MediaPlayerState.Opening:
                    IsLoading = true;
                    _timer.Stop();
                    break;
            }
        }

        private void HelperOnShutdown(object sender, EventArgs eventArgs)
        {
            CurrentQueue = null;
            NowPlayingSheetUtility.CloseNowPlaying();
            IsPlayerActive = false;
        }

        private void HelperOnTrackChanged(object sender, EventArgs eventArgs)
        {
            var playerInstance = _helper.SafeMediaPlayer;

            if (playerInstance == null)
            {
                return;
            }

            var state = _helper.SafePlayerState;

            if (state != MediaPlayerState.Closed && state != MediaPlayerState.Stopped)
            {
                if (CurrentQueue != null && CurrentQueue.Song != null)
                {
                    var lastPlayed = DateTime.Now - CurrentQueue.Song.LastPlayed;

                    if (lastPlayed.TotalSeconds > 30)
                    {
                        CurrentQueue.Song.PlayCount++;
                        CurrentQueue.Song.LastPlayed = DateTime.Now;
                    }
                }

                var currentId = _appSettingsHelper.Read<int>(PlayerConstants.CurrentTrack);
                var newQueue = _service.PlaybackQueue.FirstOrDefault(p => p.Id == currentId);

                if (CurrentQueue != newQueue)
                {
                    IsLoading = true;
                    Position = TimeSpan.Zero;

                    CurrentQueue = newQueue;

                    if (CurrentQueue != null && CurrentQueue.Song != null
                        && CurrentQueue.Song.Duration.Ticks != Duration.Ticks)
                    {
                        CurrentQueue.Song.Duration = Duration;
                    }
                }
                IsPlayerActive = true;
            }
            else
            {
                NowPlayingSheetUtility.CloseNowPlaying();
                IsPlayerActive = false;
                CurrentQueue = null;
            }

            Duration = playerInstance.NaturalDuration;
            if (Duration == TimeSpan.MinValue)
                Duration = TimeSpan.Zero;
        }

        private void NextSong()
        {
            _helper.NextSong();
        }

        private void PlayPauseToggle()
        {
            _helper.PlayPauseToggle();
        }

        private void PrevSong()
        {
            _helper.PrevSong();
        }

        private void TimerOnTick(object sender, object o)
        {
            try
            {
                var state = _helper.SafePlayerState;
                Position = state == MediaPlayerState.Opening ? TimeSpan.Zero : BackgroundMediaPlayer.Current.Position;
            }
            catch
            {
                // pertaining to the following (random) error: Server execution failed (Exception from HRESULT: 0x80080005 (CO_E_SERVER_EXEC_FAILURE))
            }
        }
    }
}