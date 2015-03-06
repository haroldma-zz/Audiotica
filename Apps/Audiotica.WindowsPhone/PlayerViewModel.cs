using System;
using System.Linq;

using Audiotica.Core;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Xamarin;

namespace Audiotica
{
    public class PlayerViewModel : ViewModelBase
    {
        private readonly IAppSettingsHelper appSettingsHelper;

        private readonly AudioPlayerHelper helper;

        private readonly RelayCommand nextRelayCommand;

        private readonly RelayCommand playPauseRelayCommand;

        private readonly RelayCommand prevRelayCommand;

        private readonly ICollectionService service;

        private readonly DispatcherTimer timer;

        private QueueSong currentQueue;

        private TimeSpan duration;

        private bool isLoading;

        private bool isPlayerActive;

        private double npbHeight = double.NaN;

        private double npHeight;

        private Symbol playPauseIcon;

        private TimeSpan position;

        public PlayerViewModel(
            AudioPlayerHelper helper, 
            ICollectionService service, 
            IAppSettingsHelper appSettingsHelper)
        {
            this.helper = helper;
            this.service = service;
            this.appSettingsHelper = appSettingsHelper;

            if (!this.IsInDesignMode)
            {
                helper.TrackChanged += this.HelperOnTrackChanged;
                helper.PlaybackStateChanged += this.HelperOnPlaybackStateChanged;
                helper.Shutdown += this.HelperOnShutdown;

                this.nextRelayCommand = new RelayCommand(this.NextSong);
                this.prevRelayCommand = new RelayCommand(this.PrevSong);
                this.playPauseRelayCommand = new RelayCommand(this.PlayPauseToggle);

                this.timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                this.timer.Tick += this.TimerOnTick;
            }
            else
            {
                this.CurrentQueue = service.PlaybackQueue.FirstOrDefault();
                this.PlayPauseIcon = Symbol.Play;
            }
        }

        public bool IsRepeat
        {
            get
            {
                return this.appSettingsHelper.Read<bool>("Repeat");
            }

            set
            {
                this.appSettingsHelper.Write("Repeat", value);
                this.RaisePropertyChanged();
            }
        }

        public bool IsShuffle
        {
            get
            {
                return this.appSettingsHelper.Read<bool>("Shuffle");
            }

            set
            {
                this.appSettingsHelper.Write("Shuffle", value);
                this.service.ShuffleModeChanged();
                this.RaisePropertyChanged();
                this.AudioPlayerHelper.OnShuffleChanged();
                Insights.Track("Shuffle", "Enabled", value ? "True" : "False");
            }
        }

        public bool IsPlayerActive
        {
            get
            {
                return this.isPlayerActive;
            }

            set
            {
                this.Set(ref this.isPlayerActive, value);
            }
        }

        public TimeSpan Duration
        {
            get
            {
                return this.duration;
            }

            set
            {
                this.Set(ref this.duration, value);
            }
        }

        public TimeSpan Position
        {
            get
            {
                return this.position;
            }

            set
            {
                this.Set(ref this.position, value);
            }
        }

        public QueueSong CurrentQueue
        {
            get
            {
                return this.currentQueue;
            }

            set
            {
                this.Set(ref this.currentQueue, value);
            }
        }

        public Symbol PlayPauseIcon
        {
            get
            {
                return this.playPauseIcon;
            }

            set
            {
                this.Set(ref this.playPauseIcon, value);
            }
        }

        public bool IsLoading
        {
            get
            {
                return this.isLoading;
            }

            set
            {
                this.Set(ref this.isLoading, value);
            }
        }

        public RelayCommand NextRelayCommand
        {
            get
            {
                return this.nextRelayCommand;
            }
        }

        public RelayCommand PrevRelayCommand
        {
            get
            {
                return this.prevRelayCommand;
            }
        }

        public RelayCommand PlayPauseRelayCommand
        {
            get
            {
                return this.playPauseRelayCommand;
            }
        }

        public double NowPlayingHeight
        {
            get
            {
                return this.npHeight;
            }

            set
            {
                this.Set(ref this.npHeight, value);
            }
        }

        public double NowPlayingBarHeight
        {
            get
            {
                return this.npbHeight;
            }

            set
            {
                this.Set(ref this.npbHeight, value);
            }
        }

        public ICollectionService CollectionService
        {
            get
            {
                return this.service;
            }
        }

        public AudioPlayerHelper AudioPlayerHelper
        {
            get
            {
                return this.helper;
            }
        }

        private void HelperOnPlaybackStateChanged(object sender, PlaybackStateEventArgs playbackStateEventArgs)
        {
            this.IsLoading = false;
            switch (playbackStateEventArgs.State)
            {
                default:
                    this.PlayPauseIcon = Symbol.Play;
                    this.timer.Stop();
                    break;
                case MediaPlayerState.Playing:
                    this.timer.Start();
                    this.PlayPauseIcon = Symbol.Pause;
                    break;
                case MediaPlayerState.Buffering:
                case MediaPlayerState.Opening:
                    this.IsLoading = true;
                    break;
            }
        }

        private void HelperOnShutdown(object sender, EventArgs eventArgs)
        {
            this.CurrentQueue = null;
            NowPlayingSheetUtility.CloseNowPlaying();
            this.IsPlayerActive = false;
        }

        private void HelperOnTrackChanged(object sender, EventArgs eventArgs)
        {
            var playerInstance = BackgroundMediaPlayer.Current;

            if (playerInstance == null)
            {
                return;
            }

            this.Duration = playerInstance.NaturalDuration;

            if (Duration == TimeSpan.MinValue)
                Duration = TimeSpan.Zero;

            var state = MediaPlayerState.Closed;

            try
            {
                state = playerInstance.CurrentState;
            }
            catch
            {
                // ignored, rare occacion where the player just throws a generic Exception
            }

            if (state != MediaPlayerState.Closed && state != MediaPlayerState.Stopped)
            {
                if (this.CurrentQueue != null && CurrentQueue.Song != null)
                {
                    var lastPlayed = DateTime.Now - this.CurrentQueue.Song.LastPlayed;

                    if (lastPlayed.TotalSeconds > 30)
                    {
                        this.CurrentQueue.Song.PlayCount++;
                        this.CurrentQueue.Song.LastPlayed = DateTime.Now;
                    }
                }

                var currentId = this.appSettingsHelper.Read<int>(PlayerConstants.CurrentTrack);
                this.CurrentQueue = this.service.PlaybackQueue.FirstOrDefault(p => p.Id == currentId);

                if (this.CurrentQueue != null && this.CurrentQueue.Song != null
                    && this.CurrentQueue.Song.Duration.Ticks != this.Duration.Ticks)
                {
                    this.CurrentQueue.Song.Duration = this.Duration;
                }

                this.IsPlayerActive = true;
            }
            else
            {
                NowPlayingSheetUtility.CloseNowPlaying();
                this.IsPlayerActive = false;
                this.CurrentQueue = null;
            }
        }

        private void NextSong()
        {
            this.helper.NextSong();
        }

        private void PlayPauseToggle()
        {
            this.helper.PlayPauseToggle();
        }

        private void PrevSong()
        {
            this.helper.PrevSong();
        }

        private void TimerOnTick(object sender, object o)
        {
            try
            {
                this.Position = BackgroundMediaPlayer.Current.Position;
            }
            catch
            {
                // pertaining to the following (random) error: Server execution failed (Exception from HRESULT: 0x80080005 (CO_E_SERVER_EXEC_FAILURE))
            }
        }
    }
}