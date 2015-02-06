#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Audiotica.Core;
using Audiotica.Data.Collection.Model;

using GalaSoft.MvvmLight.Threading;

using Windows.Foundation.Collections;
using Windows.Media.Playback;

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
            RemoveMediaPlayerEventHandlers(BackgroundMediaPlayer.Current);
            BackgroundMediaPlayer.Shutdown();
            App.Locator.AppSettingsHelper.Write(PlayerConstants.CurrentTrack, null);
        }

        public void NextSong()
        {
            var value = new ValueSet { { PlayerConstants.SkipNext, string.Empty } };
            BackgroundMediaPlayer.SendMessageToBackground(value);
        }

        public void OnAppActive()
        {
            App.Locator.AppSettingsHelper.Write(PlayerConstants.AppState, PlayerConstants.ForegroundAppActive);

            try
            {
                AddMediaPlayerEventHandlers();
            }
            catch
            {
                _isShutdown = true;
            }

            RaiseEvent(TrackChanged);
            OnPlaybackStateChanged(BackgroundMediaPlayer.Current.CurrentState);
        }

        public void OnAppSuspended()
        {
            App.Locator.AppSettingsHelper.Write(PlayerConstants.AppState, PlayerConstants.ForegroundAppSuspended);
            RemoveMediaPlayerEventHandlers(BackgroundMediaPlayer.Current);
        }

        public void OnShuffleChanged()
        {
            RaiseEvent(TrackChanged);
        }

        public void PlayPauseToggle()
        {
            switch (BackgroundMediaPlayer.Current.CurrentState)
            {
                case MediaPlayerState.Playing:
                    BackgroundMediaPlayer.Current.Pause();
                    break;
                case MediaPlayerState.Paused:
                    BackgroundMediaPlayer.Current.Play();
                    break;
            }
        }

        public async void PlaySong(QueueSong song)
        {
            if (song == null)
            {
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
            RemoveMediaPlayerEventHandlers(BackgroundMediaPlayer.Current);
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
            var player = BackgroundMediaPlayer.Current;

            // avoid duplicate events
            RemoveMediaPlayerEventHandlers(player);
            player.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            _isShutdown = false;
            await Task.Delay(250);
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
            OnPlaybackStateChanged(BackgroundMediaPlayer.Current.CurrentState);
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