#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.Media.Playback;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.RunTime;

#endregion

namespace Audiotica.WindowsPhone.Player
{
    internal class QueueManager : IDisposable
    {
        #region Private members

        private readonly ISqlService _bgSql;
        private readonly MediaPlayer _mediaPlayer;
        private readonly ISqlService _sql;
        private QueueSong _currentTrack;
        private int _currentTrackIndex = -1;

        public QueueManager()
        {
            var bgDbTypes = new List<Type>
            {
                typeof (QueueSong),
                typeof (HistoryEntry),
            };
            var bgConfig = new SqlServiceConfig
            {
                Tables = bgDbTypes,
                CurrentVersion = 1,
                Path = "player.sqldb"
            };
            _bgSql = new SqlService(bgConfig);

            var dbTypes = new List<Type>
            {
                typeof (Artist),
                typeof (Album),
                typeof (Song),
                typeof (Playlist),
                typeof (PlaylistSong)
            };
            var config = new SqlServiceConfig
            {
                Tables = dbTypes,
                CurrentVersion = 3,
                Path = "collection.sqldb"
            };

            _sql = new SqlService(config);

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

                if (_currentTrack != null)
                    return _currentTrack;

                _currentTrack = _currentTrackIndex < tracks.Count ? tracks[_currentTrackIndex] : null;
                return _currentTrack;
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

            OnTrackChanged();
        }

        private void OnTrackChanged()
        {
            var played = DateTime.Now;
            var historyItem = new HistoryEntry
            {
                DatePlayed = played,
                SongId = CurrentTrack.SongId
            };

            _bgSql.Insert(historyItem);

            CurrentTrack.Song.PlayCount++;
            CurrentTrack.Song.LastPlayed = played;

            if (CurrentTrack.Song.Duration.Ticks != _mediaPlayer.NaturalDuration.Ticks)
                CurrentTrack.Song.Duration = _mediaPlayer.NaturalDuration;

            _sql.UpdateItem(CurrentTrack.Song);
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
            RefreshTracks();
            UpdateMediaEnded();

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
            RefreshTracks();
            UpdateMediaEnded();

            var source = song.Song.AudioUrl;
            _currentTrackIndex = tracks.FindIndex(p => p.SongId == song.SongId);
            _mediaPlayer.AutoPlay = false;
            _mediaPlayer.SetUriSource(new Uri(source));
        }

        public void StartTrack(long id)
        {
            RefreshTracks();
            UpdateMediaEnded();

            var track = tracks.FirstOrDefault(p => p.Id == id);
            _currentTrackIndex = tracks.IndexOf(track);
            _mediaPlayer.AutoPlay = false;
            _mediaPlayer.SetUriSource(new Uri(track.Song.AudioUrl));
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

        private void UpdateMediaEnded()
        {
            if (CurrentTrack == null)
            {
                _bgSql.DeleteTableAsync<HistoryEntry>().Wait();
                return;
            }

            var historyItem = _bgSql.SelectAll<HistoryEntry>().FirstOrDefault(p => p.SongId == CurrentTrack.SongId);
            if (historyItem != null)
            {
                historyItem.DateEnded = DateTime.Now;
                historyItem.CanScrobble = true;
                _bgSql.UpdateItem(historyItem);
            }

            _currentTrack = null;
        }

        /// <summary>
        ///     Skip to next track
        /// </summary>
        public void SkipToPrevious()
        {
            if (_currentTrackIndex == 0)
            {
                StartTrackAt(tracks.Count - 1);
            }
            else
            {
                StartTrackAt(_currentTrackIndex - 1);
            }
        }

        #endregion

        public void Dispose()
        {
            _bgSql.Dispose();
            _sql.Dispose();
        }

        private void RefreshTracks()
        {
            var collectionService = new CollectionService(_sql, _bgSql, null);
            collectionService.LoadLibrary();
            tracks = collectionService.PlaybackQueue.ToList();
        }
    }
}