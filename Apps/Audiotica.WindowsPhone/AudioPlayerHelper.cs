#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Audiotica.Core;
using Audiotica.Data.Collection.Model;

using GalaSoft.MvvmLight.Threading;

using Windows.Foundation.Collections;
using Windows.Media.Playback;

using Audiotica.Core.WinRt.Common;

using Xamarin;

#endregion

namespace Audiotica
{
    public class AudioPlayerHelper
    {
        private bool _isShutdown = true;

        public event EventHandler Shutdown;

        public event EventHandler TrackChanged;

        public event EventHandler<PlaybackStateEventArgs> PlaybackStateChanged;

        public void FullShutdown()
        {
            var player = SafeMediaPlayer;

            if (player == null)
            {
                return;
            }

            RemoveMediaPlayerEventHandlers(player);
            BackgroundMediaPlayer.Shutdown();
            App.Locator.AppSettingsHelper.Write(PlayerConstants.CurrentTrack, null);
        }

        private MediaPlayer _player;
        public MediaPlayer SafeMediaPlayer
        {
            get
            {
                try
                {
                    return _player ?? (_player = BackgroundMediaPlayer.Current);
                }
                catch
                {
                    _player = null;
                    return null;
                }
            }
        }

        public MediaPlayerState SafePlayerState
        {
            get
            {
                var player = SafeMediaPlayer;

                try
                {
                    return player == null ? MediaPlayerState.Closed : player.CurrentState;
                }
                catch
                {
                    return MediaPlayerState.Closed;
                }
            }
        }

        public void NextSong()
        {
            var value = new ValueSet { { PlayerConstants.SkipNext, string.Empty } };
            BackgroundMediaPlayer.SendMessageToBackground(value);
        }

        public void OnAppActive()
        {
            App.Locator.AppSettingsHelper.Write(PlayerConstants.AppState, PlayerConstants.ForegroundAppActive);

            AddMediaPlayerEventHandlers();
            RaiseEvent(TrackChanged);
            OnPlaybackStateChanged(SafePlayerState);
        }

        public void OnAppSuspended()
        {
            App.Locator.AppSettingsHelper.Write(PlayerConstants.AppState, PlayerConstants.ForegroundAppSuspended);
            RemoveMediaPlayerEventHandlers(SafeMediaPlayer);
        }

        public void OnShuffleChanged()
        {
            RaiseEvent(TrackChanged);
        }

        public void PlayPauseToggle()
        {
            var player = SafeMediaPlayer;

            if (player == null)
            {
                return;
            }

            switch (SafePlayerState)
            {
                case MediaPlayerState.Playing:
                    player.Pause();
                    break;
                case MediaPlayerState.Paused:
                    player.Play();
                    break;
            }
        }

        public async void PlaySong(QueueSong song)
        {
            if (song == null || song.Song == null)
            {
                CurtainPrompt.ShowError("Song seems to be empty...");
                return;
            }

            Insights.Track(
                "Play Song", 
                new Dictionary<string, string>
                {
                    { "Name", song.Song.Name }, 
                    { "ArtistName", song.Song.ArtistName }, 
                    { "ProviderId", song.Song.ProviderId }
                });

            if (_isShutdown)
            {
                await AddMediaPlayerEventHandlers();
            }

            App.Locator.AppSettingsHelper.Write(PlayerConstants.CurrentTrack, song.Id);

            var message = new ValueSet { { PlayerConstants.StartPlayback, null } };
            BackgroundMediaPlayer.SendMessageToBackground(message);

            RaiseEvent(TrackChanged);
        }

        public void PrevSong()
        {
            var value = new ValueSet { { PlayerConstants.SkipPrevious, string.Empty } };
            BackgroundMediaPlayer.SendMessageToBackground(value);
        }

        public async Task ShutdownPlayerAsync()
        {
            var player = SafeMediaPlayer;

            if (player == null)
            {
                return;
            }

            RemoveMediaPlayerEventHandlers(player);
            BackgroundMediaPlayer.Shutdown();
            App.Locator.AppSettingsHelper.Write(PlayerConstants.CurrentTrack, null);
            await Task.Delay(500);
            _isShutdown = true;
            RaiseEvent(Shutdown);
        }

        protected virtual void OnPlaybackStateChanged(MediaPlayerState state)
        {
            DispatcherHelper.RunAsync(
                () =>
                {
                    var handler = PlaybackStateChanged;
                    if (handler != null)
                    {
                        handler(this, new PlaybackStateEventArgs(state));
                    }
                });
        }

        private async Task AddMediaPlayerEventHandlers()
        {
            var player = SafeMediaPlayer;

            if (player == null)
            {
                return;
            }

            try
            {
                // avoid duplicate events
                RemoveMediaPlayerEventHandlers(player);
                player.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
                _isShutdown = false;
                await Task.Delay(250);
            }
            catch
            {
                // ignored
                _isShutdown = true;
            }
        }

        private void BackgroundMediaPlayer_MessageReceivedFromBackground(
            object sender, 
            MediaPlayerDataReceivedEventArgs e)
        {
            foreach (var key in e.Data.Keys)
            {
                switch (key)
                {
                    case PlayerConstants.Trackchanged:
                        RaiseEvent(TrackChanged);
                        break;
                }
            }
        }

        private void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            OnPlaybackStateChanged(SafePlayerState);
        }

        private void RaiseEvent(EventHandler handler)
        {
            DispatcherHelper.RunAsync(
                () =>
                {
                    if (handler != null)
                    {
                        handler(this, EventArgs.Empty);
                    }
                });
        }

        private void RemoveMediaPlayerEventHandlers(MediaPlayer player)
        {
            player.CurrentStateChanged -= MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground -= BackgroundMediaPlayer_MessageReceivedFromBackground;
        }
    }

    public class PlaybackStateEventArgs : EventArgs
    {
        public PlaybackStateEventArgs(MediaPlayerState state)
        {
            State = state;
        }

        public MediaPlayerState State { get; set; }
    }
}