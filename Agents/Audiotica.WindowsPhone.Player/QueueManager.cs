#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.Media.Playback;
using Audiotica.Data.Collection.Model;

#endregion

namespace Audiotica.WindowsPhone.Player
{
    internal class QueueManager
    {
        #region Private members

        private readonly MediaPlayer _mediaPlayer;
        private int _currentTrackIndex = -1;

        public QueueManager(List<QueueSong> songs)
        {
            tracks = songs;
            _mediaPlayer = BackgroundMediaPlayer.Current;
            _mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            _mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            _mediaPlayer.CurrentStateChanged += mediaPlayer_CurrentStateChanged;
            _mediaPlayer.MediaFailed += mediaPlayer_MediaFailed;
        }

        private List<QueueSong> tracks { get; set; }

        #endregion

        #region Public properties, events and handlers

        /// <summary>
        ///     Get the current track
        /// </summary>
        public QueueSong CurrentTrack
        {
            get
            {
                if (_currentTrackIndex == -1)
                    return null;
                if (_currentTrackIndex < tracks.Count)
                    return tracks[_currentTrackIndex];
                throw new Exception("Damn, high track number!!!");
            }
        }

        /// <summary>
        ///     Invoked when the media player is ready to move to next track
        /// </summary>
        public event TypedEventHandler<QueueManager, object> TrackChanged;

        #endregion

        #region MediaPlayer Handlers

        /// <summary>
        ///     Handler for state changed event of Media Player
        /// </summary>
        private void mediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (sender.CurrentState == MediaPlayerState.Playing)
            {
                sender.Volume = 1.0;
                //sender.PlaybackMediaMarkers.Clear();
            }
        }

        /// <summary>
        ///     Fired when MediaPlayer is ready to play the track
        /// </summary>
        private void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            // wait for media to be ready
            sender.Play();

            if (CurrentTrack == null) return;

            Debug.WriteLine("New Track" + CurrentTrack.Song.Name);

            if (TrackChanged != null)
                TrackChanged.Invoke(this, CurrentTrack.SongId);
        }

        /// <summary>
        ///     Handler for MediaPlayer Media Ended
        /// </summary>
        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            SkipToNext();
        }

        private void mediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            Debug.WriteLine("Failed with error code " + args.ExtendedErrorCode);
        }

        #endregion

        #region Playlist command handlers

        /// <summary>
        ///     Starts track at given position in the track list
        /// </summary>
        private void StartTrackAt(int id)
        {
            var source = tracks[id].Song.AudioUrl;
            _currentTrackIndex = id;
            _mediaPlayer.AutoPlay = false;
            _mediaPlayer.SetUriSource(new Uri(source));
        }

        /// <summary>
        ///     Starts a given track
        /// </summary>
        public void StartTrack(QueueSong song)
        {
            var source = song.Song.AudioUrl;
            _currentTrackIndex = tracks.FindIndex(p => p.SongId == song.SongId);
            _mediaPlayer.AutoPlay = false;
            _mediaPlayer.SetUriSource(new Uri(source));
        }

        public void StartTrack(long id)
        {
            var source = tracks.FirstOrDefault(p => p.SongId == id).Song.AudioUrl;
            _currentTrackIndex = tracks.FindIndex(p => p.SongId == id);
            _mediaPlayer.AutoPlay = false;
            _mediaPlayer.SetUriSource(new Uri(source));
        }

        /// <summary>
        ///     Play all tracks in the list starting with 0
        /// </summary>
        public void PlayAllTracks()
        {
            StartTrackAt(0);
        }

        /// <summary>
        ///     Skip to next track
        /// </summary>
        public void SkipToNext()
        {
            StartTrackAt((_currentTrackIndex + 1)%tracks.Count);
        }

        /// <summary>
        ///     Skip to next track
        /// </summary>
        public void SkipToPrevious()
        {
            if (_currentTrackIndex == 0)
            {
                StartTrackAt(_currentTrackIndex);
            }
            else
            {
                StartTrackAt(_currentTrackIndex - 1);
            }
        }

        #endregion
    }
}