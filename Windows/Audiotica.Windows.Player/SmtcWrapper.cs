using System;
using System.Diagnostics;
using Windows.Media;
using Windows.Media.Playback;

namespace Audiotica.Windows.Player
{
    internal class SmtcWrapper : IDisposable
    {
        private SystemMediaTransportControls _smtc;

        public SmtcWrapper(SystemMediaTransportControls smtc)
        {
            _smtc = smtc;
            smtc.IsEnabled = true;
            smtc.IsPauseEnabled = true;
            smtc.IsPlayEnabled = true;
            smtc.IsNextEnabled = true;
            smtc.IsPreviousEnabled = true;
            Subscribe();
        }

        public MediaPlaybackStatus PlaybackStatus
        {
            get { return _smtc.PlaybackStatus; }
            set { _smtc.PlaybackStatus = value; }
        }

        public void Dispose()
        {
            Unsubscribe();
            _smtc = null;
        }

        /// <summary>
        ///     Update Universal Volume Control (UVC) using SystemMediaTransPortControl APIs
        /// </summary>
        public void UpdateUvcOnNewTrack(MediaPlaybackItem item)
        {
            if (item == null)
            {
                _smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
                _smtc.DisplayUpdater.MusicProperties.Title = string.Empty;
                _smtc.DisplayUpdater.MusicProperties.Artist = string.Empty;
                _smtc.DisplayUpdater.MusicProperties.AlbumTitle = string.Empty;
                _smtc.DisplayUpdater.MusicProperties.AlbumArtist = string.Empty;
                _smtc.DisplayUpdater.Update();
                return;
            }

            _smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
            _smtc.DisplayUpdater.Type = MediaPlaybackType.Music;
            _smtc.DisplayUpdater.MusicProperties.Title = item.Source.Title();
            _smtc.DisplayUpdater.MusicProperties.Artist = item.Source.Artists();
            _smtc.DisplayUpdater.MusicProperties.AlbumTitle = item.Source.AlbumTitle();
            _smtc.DisplayUpdater.MusicProperties.AlbumArtist = item.Source.AlbumArtist();

            /*  var albumArtUri = item.Source.CustomProperties[AlbumArtKey] as Uri;
              if (albumArtUri != null)
                  smtc.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(albumArtUri);
              else
                  smtc.DisplayUpdater.Thumbnail = null;*/

            _smtc.DisplayUpdater.Update();
        }

        public event EventHandler PlayPressed;
        public event EventHandler PausePressed;
        public event EventHandler NextPressed;
        public event EventHandler PrevPressed;

        private void Subscribe()
        {
            Unsubscribe();
            _smtc.ButtonPressed += smtc_ButtonPressed;
            _smtc.PropertyChanged += smtc_PropertyChanged;
        }

        private void Unsubscribe()
        {
            _smtc.ButtonPressed -= smtc_ButtonPressed;
            _smtc.PropertyChanged -= smtc_PropertyChanged;
        }

        /// <summary>
        ///     This function controls the button events from UVC.
        ///     This code if not run in background process, will not be able to handle button pressed events when app is suspended.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void smtc_ButtonPressed(SystemMediaTransportControls sender,
            SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    Debug.WriteLine("UVC play button pressed");
                    PlayPressed?.Invoke(this, null);
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    Debug.WriteLine("UVC pause button pressed");
                    PausePressed?.Invoke(this, null);
                    break;
                case SystemMediaTransportControlsButton.Next:
                    Debug.WriteLine("UVC next button pressed");
                    NextPressed?.Invoke(this, null);
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    Debug.WriteLine("UVC previous button pressed");
                    PrevPressed?.Invoke(this, null);
                    break;
            }
        }

        /// <summary>
        ///     Fires when any SystemMediaTransportControl property is changed by system or user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void smtc_PropertyChanged(SystemMediaTransportControls sender,
            SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            // TODO: If soundlevel turns to muted, app can choose to pause the music
        }
    }
}