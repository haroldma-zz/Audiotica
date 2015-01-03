#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.Media.Playback;
using Audiotica.Core.Utilities;
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
                CurrentVersion = 6,
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

            if (CurrentTrack.Song.Duration.Ticks == _mediaPlayer.NaturalDuration.Ticks) return;

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
            if (tracks == null)
                RefreshTracks();

            UpdateMediaEnded();

            var track = tracks[id];
            _currentTrackIndex = id;
            _mediaPlayer.AutoPlay = false;

            if (track.Song.IsStreaming)
            {
                _mediaPlayer.SetUriSource(new Uri(track.Song.AudioUrl));
            }
            else
            {
                var file =
                    StorageHelper.GetFileAsync(string.Format("songs/{0}.mp3", track.SongId)).Result;
                _mediaPlayer.SetFileSource(file);
            }
        }

        /// <summary>
        ///     Starts a given track
        /// </summary>
        public void StartTrack(QueueSong track)
        {
            if (tracks == null)
                RefreshTracks();

            UpdateMediaEnded();

            _currentTrackIndex = tracks.FindIndex(p => p.SongId == track.SongId);
            _mediaPlayer.AutoPlay = false;

            if (track.Song.IsStreaming)
            {
                _mediaPlayer.SetUriSource(new Uri(track.Song.AudioUrl));
            }
            else
            {
                var file =
                    StorageHelper.GetFileAsync(string.Format("songs/{0}.mp3", track.SongId)).Result;
                _mediaPlayer.SetFileSource(file);
            }
        }

        public void StartTrack(long id)
        {
            if (tracks == null)
                RefreshTracks();

            UpdateMediaEnded();

            var track = tracks.FirstOrDefault(p => p.Id == id);
            _currentTrackIndex = tracks.IndexOf(track);
            _mediaPlayer.AutoPlay = false;

            if (track.Song.IsStreaming)
            {
                _mediaPlayer.SetUriSource(new Uri(track.Song.AudioUrl));
            }
            else
            {
                var file =
                    StorageHelper.GetFileAsync(string.Format("songs/{0}.mp3", track.SongId)).Result;
                _mediaPlayer.SetFileSource(file);
            }
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
            var history = _bgSql.SelectAll<HistoryEntry>();

            //if null then the player has just been launched
            if (CurrentTrack == null)
            {
                //reset the incrementable Id of the table
                if (history.Count(p => p.CanScrobble) == 0)
                    _bgSql.DeleteTableAsync<HistoryEntry>().Wait();
                return;
            }

            var historyItem = history.FirstOrDefault(p => p.SongId == CurrentTrack.SongId);
            if (historyItem != null)
            {
                var playbackTime = _mediaPlayer.Position.TotalSeconds;
                var duration = _mediaPlayer.NaturalDuration.TotalSeconds;

                /* When is a scrobble a scrobble?
                 * A track should only be scrobbled when the following conditions have been met:
                 * 1. The track must be longer than 30 seconds.
                 * 2. And the track has been played for at least half its duration, or for 4 minutes (whichever occurs earlier.)
                 */

                if (duration >= 30 
                    && (playbackTime >= duration/2 || playbackTime >= 60*4))
                {
                    CurrentTrack.Song.PlayCount++;
                    CurrentTrack.Song.LastPlayed = historyItem.DatePlayed;

                    if (CurrentTrack.Song.Duration.Ticks != _mediaPlayer.NaturalDuration.Ticks)
                        CurrentTrack.Song.Duration = _mediaPlayer.NaturalDuration;

                    _sql.UpdateItem(CurrentTrack.Song);

                    historyItem.CanScrobble = true;
                    _bgSql.UpdateItem(historyItem);
                }
                else
                {
                    //not a scrobble
                    _bgSql.DeleteItemAsync(historyItem);
                }
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

        public void RefreshTracks()
        {
            var collectionService = new CollectionService(_sql, _bgSql, null);
            collectionService.LoadLibrary(true);
            tracks = collectionService.PlaybackQueue.ToList();
        }
    }
}