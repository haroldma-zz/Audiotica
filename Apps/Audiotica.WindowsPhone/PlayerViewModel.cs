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
using Audiotica.Data.Service.Interfaces;
using Audiotica.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using IF.Lastfm.Core.Api.Enums;

#endregion

namespace Audiotica
{
    public class PlayerViewModel : ViewModelBase
    {
        private readonly ISqlService _bgSqlService;
        private readonly AudioPlayerHelper _helper;
        private readonly RelayCommand _nextRelayCommand;
        private readonly RelayCommand _playPauseRelayCommand;
        private readonly RelayCommand _prevRelayCommand;
        private readonly IScrobblerService _scrobblerService;
        private readonly ICollectionService _service;
        private readonly DispatcherTimer _timer;
        private QueueSong _currentQueue;
        private TimeSpan _duration;
        private bool _isLoading;
        private bool _isUpdating;
        private RelayCommand _npBarTappedRelayCommand;
        private double _npHeight;
        private double _npbHeight = double.NaN;
        private IconElement _playPauseIcon;
        private TimeSpan _position;

        public PlayerViewModel(AudioPlayerHelper helper, ICollectionService service, ISqlService bgSqlService,
            IScrobblerService scrobblerService)
        {
            _helper = helper;
            _service = service;
            _bgSqlService = bgSqlService;
            _scrobblerService = scrobblerService;

            if (!IsInDesignMode)
            {
                helper.TrackChanged += HelperOnTrackChanged;
                helper.PlaybackStateChanged += HelperOnPlaybackStateChanged;
                helper.Shutdown += HelperOnShutdown;

                _nextRelayCommand = new RelayCommand(NextSong);
                _prevRelayCommand = new RelayCommand(PrevSong);
                _playPauseRelayCommand = new RelayCommand(PlayPauseToggle);
                _npBarTappedRelayCommand = new RelayCommand(NowPlayingBarTapped);

                _timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
                _timer.Tick += TimerOnTick;
            }
            else
            {
                CurrentQueue = service.PlaybackQueue.FirstOrDefault();
                PlayPauseIcon = new SymbolIcon(Symbol.Play);
            }
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

        public RelayCommand NpBarTappedRelayCommand
        {
            get { return _npBarTappedRelayCommand; }
            set { Set(ref _npBarTappedRelayCommand, value); }
        }

        private void NowPlayingBarTapped()
        {
            App.RootFrame.Navigate(typeof (NowPlayingPage));
        }

        private void TimerOnTick(object sender, object o)
        {
            Position = BackgroundMediaPlayer.Current.Position;
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
                var currentId = AppSettingsHelper.Read<long>(PlayerConstants.CurrentTrack);
                CurrentQueue = _service.PlaybackQueue.FirstOrDefault(p => p.Id == currentId);

                if (CurrentQueue != null
                    && CurrentQueue.Song != null
                    && CurrentQueue.Song.Duration.Ticks != Duration.Ticks)
                    CurrentQueue.Song.Duration = Duration;
            }
            else
            {
                CurrentQueue = null;
            }

            ScrobbleHistory();
        }

        private async void ScrobbleHistory()
        {
            if (_isUpdating) return;

            _isUpdating = true;

            var scrobble = AppSettingsHelper.Read<bool>("Scrobble");

            #region Update now playing (last.fm)

            if (CurrentQueue != null && scrobble)
            {
                var npAlbumName = CurrentQueue.Song.Album.ProviderId.StartsWith("autc.single.")
                    ? ""
                    : CurrentQueue.Song.Album.Name;
                var npArtistName = string.IsNullOrEmpty(npAlbumName) ? "" : CurrentQueue.Song.Artist.Name;

                await
                    _scrobblerService.ScrobbleNowPlayingAsync(CurrentQueue.Song.Name, CurrentQueue.Song.ArtistName,
                        DateTime.UtcNow, CurrentQueue.Song.Duration, npAlbumName, npArtistName);
            }

            #endregion

            #region Scrobble

            var history = await _service.FetchHistoryAsync();

            foreach (var historyEntry in history)
            {
                if (historyEntry.Song.LastPlayed < historyEntry.DatePlayed)
                {
                    historyEntry.Song.PlayCount++;
                    historyEntry.Song.LastPlayed = historyEntry.DatePlayed;
                }

                if (!historyEntry.Scrobbled && historyEntry.CanScrobble && scrobble)
                {
                    var diff = historyEntry.DateEnded - historyEntry.DatePlayed;

                    //only scrobbled when is has been playing for 30 secs or more
                    if (diff.TotalSeconds >= 30)
                    {
                        var albumName = historyEntry.Song.Album.ProviderId.StartsWith("autc.single.")
                            ? ""
                            : historyEntry.Song.Album.Name;
                        var artistName = string.IsNullOrEmpty(albumName) ? "" : historyEntry.Song.Artist.Name;

                        var result =
                            await
                                _scrobblerService.ScrobbleAsync(historyEntry.Song.Name, historyEntry.Song.ArtistName,
                                    historyEntry.DatePlayed.ToUniversalTime(), historyEntry.Song.Duration, albumName,
                                    artistName);

                        //if no error happened, or there was a failure (unrecoverable), then mark as scrobled
                        historyEntry.Scrobbled = result == LastFmApiError.None || result == LastFmApiError.Failure;
                    }
                    else
                    {
                        //else mark it for delete
                        historyEntry.Scrobbled = true;
                    }
                }

                if (!historyEntry.Scrobbled && scrobble) continue;

                //if scrobbled the history item, then delete it
                if (historyEntry.CanScrobble)
                    await _bgSqlService.DeleteItemAsync(historyEntry);
            }

            #endregion

            _isUpdating = false;
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