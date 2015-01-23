#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Audiotica.Core;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection.Model;
using GalaSoft.MvvmLight.Threading;
using Xamarin;

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
            var player = BackgroundMediaPlayer.Current;

            //avoid duplicate events
            RemoveMediaPlayerEventHandlers(player);
            player.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        private void RemoveMediaPlayerEventHandlers(MediaPlayer player)
        {
            player.CurrentStateChanged -= MediaPlayer_CurrentStateChanged;
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

        public void OnShuffleChanged()
        {
            RaiseEvent(TrackChanged);
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

        public void PlaySong(QueueSong song)
        {
            if (song == null)
                return;

            Insights.Track("Play Song", new Dictionary<string, string>
            {
                {"Name",song.Song.Name},
                {"ArtistName",song.Song.ArtistName},
                {"ProviderId",song.Song.ProviderId}
            });

            if (_isShutdown)
                AddMediaPlayerEventHandlers();

            App.Locator.AppSettingsHelper.Write(PlayerConstants.CurrentTrack, song.Id);
            
            var message = new ValueSet {{PlayerConstants.StartPlayback, null}};
            BackgroundMediaPlayer.SendMessageToBackground(message);

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

        public void FullShutdown()
        {
            RemoveMediaPlayerEventHandlers(BackgroundMediaPlayer.Current);
            BackgroundMediaPlayer.Shutdown();
            App.Locator.AppSettingsHelper.Write(PlayerConstants.CurrentTrack, null);
        }

        public async Task ShutdownPlayerAsync()
        {
            RemoveMediaPlayerEventHandlers(BackgroundMediaPlayer.Current);
            BackgroundMediaPlayer.Shutdown();
            App.Locator.AppSettingsHelper.Write(PlayerConstants.CurrentTrack, null);
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