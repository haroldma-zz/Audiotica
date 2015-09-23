using System;
using System.Linq;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Audiotica.Database.Models;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    public class PlayerBarViewModel : ViewModelBase
    {
        private readonly IPlayerService _playerService;
        private readonly DispatcherTimer _timer;
        private QueueTrack _currentQueueTrack;
        private double _playbackDuration;
        private double _playbackPosition;
        private Symbol _playPauseIcon = Symbol.Play;

        public PlayerBarViewModel(IPlayerService playerService)
        {
            _playerService = playerService;
            _playerService.TrackChanged += PlayerServiceOnTrackChanged;
            _playerService.MediaStateChanged += PlayerServiceOnMediaStateChanged;

            PlayPauseCommand = new Command(() => _playerService.PlayOrPause());
            NextCommand = new Command(() => _playerService.Next());
            PrevCommand = new Command(() => _playerService.Previous());

            _timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(500)};
            _timer.Tick += TimerOnTick;
        }

        public Command PrevCommand { get; }

        public Command NextCommand { get; }

        public Command PlayPauseCommand { get; }

        public QueueTrack CurrentQueueTrack
        {
            get { return _currentQueueTrack; }
            set { Set(ref _currentQueueTrack, value); }
        }

        public Symbol PlayPauseIcon
        {
            get { return _playPauseIcon; }
            set { Set(ref _playPauseIcon, value); }
        }

        public double PlaybackPosition
        {
            get { return _playbackPosition; }
            set { Set(ref _playbackPosition, value); }
        }

        public double PlaybackDuration
        {
            get { return _playbackDuration; }
            set { Set(ref _playbackDuration, value); }
        }

        private void TimerOnTick(object sender, object o)
        {
            PlaybackPosition = BackgroundMediaPlayer.Current.Position.TotalMilliseconds;
            PlaybackDuration = BackgroundMediaPlayer.Current.NaturalDuration.TotalMilliseconds;
        }

        private void PlayerServiceOnMediaStateChanged(object sender, MediaPlayerState mediaPlayerState)
        {
            var icon = Symbol.Play;
            switch (mediaPlayerState)
            {
                case MediaPlayerState.Playing:
                    icon = Symbol.Pause;
                    _timer.Start();
                    break;
                default:
                    _timer.Stop();
                    break;
            }
            PlayPauseIcon = icon;
        }

        private void PlayerServiceOnTrackChanged(object sender, string s)
        {
            CurrentQueueTrack =
                _playerService.PlaybackQueue.FirstOrDefault(queueTrack => queueTrack.Id == s);
        }
    }
}