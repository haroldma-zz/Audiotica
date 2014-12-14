#region

using System;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Audiotica.Core;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection.Model;
using GalaSoft.MvvmLight.Threading;

#endregion

namespace Audiotica
{
    public class AudioPlayerHelper
    {
        private bool _isShutdown;
        public event EventHandler Shutdown;
        public event EventHandler TrackChanged;
        public event EventHandler<PlaybackStateEventArgs> PlaybackStateChanged;

        protected virtual void OnPlaybackStateChanged(MediaPlayerState state)
        {
            DispatcherHelper.RunAsync(() =>
            {
                var handler = PlaybackStateChanged;
                if (handler != null) handler(this, new PlaybackStateEventArgs(state));
            });
        }

        private void RaiseEvent(EventHandler handler)
        {
            DispatcherHelper.RunAsync(() =>
            {
                if (handler != null) handler(this, EventArgs.Empty);
            });
        }

        private void AddMediaPlayerEventHandlers()
        {
            //avoid duplicate events
            RemoveMediaPlayerEventHandlers();
            BackgroundMediaPlayer.Current.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        private void RemoveMediaPlayerEventHandlers()
        {
            BackgroundMediaPlayer.Current.CurrentStateChanged -= MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground -= BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        private void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            OnPlaybackStateChanged(BackgroundMediaPlayer.Current.CurrentState);
        }

        private void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender,
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

        public void OnAppActive()
        {
            AppSettingsHelper.Write(PlayerConstants.AppState, PlayerConstants.ForegroundAppActive);
            AddMediaPlayerEventHandlers();
            RaiseEvent(TrackChanged);
            OnPlaybackStateChanged(BackgroundMediaPlayer.Current.CurrentState);
        }

        public void OnAppSuspended()
        {
            AppSettingsHelper.Write(PlayerConstants.AppState, PlayerConstants.ForegroundAppSuspended);
            RemoveMediaPlayerEventHandlers();
        }

        public void PlaySong(QueueSong song)
        {
            if (_isShutdown)
                AddMediaPlayerEventHandlers();

            AppSettingsHelper.Write(PlayerConstants.CurrentTrack, song.Id);

            var message = new ValueSet {{PlayerConstants.StartPlayback, null}};
            BackgroundMediaPlayer.SendMessageToBackground(message);

            song.Song.PlayCount++;
            song.Song.LastPlayed = DateTime.Now;
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

        public void PrevSong()
        {
            var value = new ValueSet {{PlayerConstants.SkipPrevious, ""}};
            BackgroundMediaPlayer.SendMessageToBackground(value);
        }

        public void NextSong()
        {
            var value = new ValueSet {{PlayerConstants.SkipNext, ""}};
            BackgroundMediaPlayer.SendMessageToBackground(value);
        }

        public async Task ShutdownPlayerAsync()
        {
            RemoveMediaPlayerEventHandlers();
            BackgroundMediaPlayer.Shutdown();
            AppSettingsHelper.Write(PlayerConstants.CurrentTrack, null);
            await Task.Delay(1000);
            _isShutdown = true;
            RaiseEvent(Shutdown);
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