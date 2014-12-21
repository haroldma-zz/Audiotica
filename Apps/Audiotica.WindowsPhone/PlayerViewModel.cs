#region

using System;
using System.Linq;
using Windows.Media.Playback;
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
    public class PlayerViewModel : ViewModelBase
    {
        private readonly AudioPlayerHelper _helper;
        private readonly RelayCommand _nextRelayCommand;
        private readonly RelayCommand _playPauseRelayCommand;
        private readonly RelayCommand _prevRelayCommand;
        private readonly ICollectionService _service;
        private QueueSong _currentQueue;
        private TimeSpan _duration;
        private bool _isLoading;
        private IconElement _playPauseIcon;
        private readonly DispatcherTimer _timer;
        private TimeSpan _position;

        public PlayerViewModel(AudioPlayerHelper helper, ICollectionService service)
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += TimerOnTick;

            _helper = helper;
            helper.TrackChanged += HelperOnTrackChanged;
            helper.PlaybackStateChanged += HelperOnPlaybackStateChanged;
            helper.Shutdown += HelperOnShutdown;
            _service = service;

            _nextRelayCommand = new RelayCommand(NextSong);
            _prevRelayCommand = new RelayCommand(PrevSong);
            _playPauseRelayCommand = new RelayCommand(PlayPauseToggle);

            if (!IsInDesignMode) return;

            CurrentQueue = service.PlaybackQueue.FirstOrDefault();
            PlayPauseIcon = new SymbolIcon(Symbol.Play);
        }

        private void TimerOnTick(object sender, object o)
        {
            Position = BackgroundMediaPlayer.Current.Position;
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

        public IconElement PlayPauseIcon
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

        private void HelperOnShutdown(object sender, EventArgs eventArgs)
        {
            CurrentQueue = null;
        }

        private void HelperOnPlaybackStateChanged(object sender, PlaybackStateEventArgs playbackStateEventArgs)
        {
            IsLoading = false;
            switch (playbackStateEventArgs.State)
            {
                default:
                    PlayPauseIcon = new SymbolIcon(Symbol.Play);
                    _timer.Stop();
                    break;
                case MediaPlayerState.Playing:
                    _timer.Start();
                    PlayPauseIcon = new SymbolIcon(Symbol.Pause);
                    break;
                case MediaPlayerState.Buffering:
                case MediaPlayerState.Opening:
                    IsLoading = true;
                    break;
            }
        }

        private void HelperOnTrackChanged(object sender, EventArgs eventArgs)
        {
            Duration = BackgroundMediaPlayer.Current.NaturalDuration;

            if (BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Closed &&
                BackgroundMediaPlayer.Current.CurrentState != MediaPlayerState.Stopped)
            {
                var id = AppSettingsHelper.Read<long>(PlayerConstants.CurrentTrack);
                CurrentQueue = _service.PlaybackQueue.FirstOrDefault(p => p.Id == id);

                if (CurrentQueue.Song.Duration.Ticks != Duration.Ticks)
                    CurrentQueue.Song.Duration = Duration;
            }
            else
            {
                CurrentQueue = null;
            }
        }

        private void PlayPauseToggle()
        {
            _helper.PlayPauseToggle();
        }

        private void PrevSong()
        {
            _helper.PrevSong();
        }

        private void NextSong()
        {
            _helper.NextSong();
        }
    }
}