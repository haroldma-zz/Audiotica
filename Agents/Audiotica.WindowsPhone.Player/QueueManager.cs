#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.Storage;
using Audiotica.Core;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.RunTime;
using Audiotica.Data.Service.RunTime;
using IF.Lastfm.Core.Api.Enums;

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

        public QueueManager()
        {
            _scrobbler = new ScrobblerHelper();

            var bgDbTypes = new List<Type>
            {
                typeof (QueueSong),
                typeof (HistoryEntry),
            };
            var bgConfig = new SqlServiceConfig
            {
                Tables = bgDbTypes,
                CurrentVersion = 2,
                Path = "player.sqldb"
            };
            _bgSql = new SqlService(bgConfig);
            _bgSql.Initialize();
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
            _sql.Initialize();

            _mediaPlayer = BackgroundMediaPlayer.Current;
            _mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            _mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            _mediaPlayer.CurrentStateChanged += mediaPlayer_CurrentStateChanged;
            _mediaPlayer.MediaFailed += mediaPlayer_MediaFailed;
        }

        #endregion

        #region Public properties, events and handlers

        /// <summary>
        ///     Get the current track
        /// </summary>
        public QueueSong CurrentTrack
        {
            get
            {
                return _currentTrack ?? (_currentTrack = GetCurrentQueueSong());
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
            _retryCount = 0;

            // wait for media to be ready
            sender.Play();

            if (CurrentTrack == null) return;

            Debug.WriteLine("New Track" + CurrentTrack.Song.Name);

            if (TrackChanged != null)
                TrackChanged.Invoke(this, CurrentTrack.SongId);

            OnTrackChanged();
        }

        private async void OnTrackChanged()
        {
            var played = DateTime.Now;
            var historyItem = new HistoryEntry
            {
                DatePlayed = played,
                SongId = CurrentTrack.SongId
            };

            await _bgSql.InsertAsync(historyItem);

            if (CurrentTrack.Song.Duration.Ticks == _mediaPlayer.NaturalDuration.Ticks) return;

            CurrentTrack.Song.Duration = _mediaPlayer.NaturalDuration;
            await _sql.UpdateItemAsync(CurrentTrack.Song);

            if (_scrobbler.IsScrobblingEnabled())
                await _scrobbler.UpdateNowPlaying(CurrentTrack);
        }

        /// <summary>
        ///     Handler for MediaPlayer Media Ended
        /// </summary>
        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            if (AppSettingsHelper.Read<bool>("Repeat"))
                StartTrack(GetCurrentQueueSong());

            else
                SkipToNext();
        }

        private void mediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            Debug.WriteLine("Failed with error code " + args.ExtendedErrorCode);

            if (_retryCount >= 5) return;

            SkipToNext();
            _retryCount++;
        }

        private int _retryCount;
        private readonly ScrobblerHelper _scrobbler;

        #endregion

        #region Playlist command handlers

        public void StartTrack(QueueSong track)
        {
            if (track == null)
                return;

            if (_scrobbler.IsScrobblingEnabled())
                ScrobbleOnMediaEnded(_currentTrack);

            _currentTrack = track;
            _mediaPlayer.AutoPlay = false;

            if (track.Song.IsStreaming)
            {
                _mediaPlayer.SetUriSource(new Uri(track.Song.AudioUrl));
            }
            else
            {
                var isLocal = track.Song.SongState == SongState.Local;

                StorageFile file = null;

                if (isLocal)
                {
                    if (StorageHelper.FileExistsAsync(track.Song.AudioUrl, KnownFolders.MusicLibrary).Result)
                        file = StorageHelper.GetFileAsync(track.Song.AudioUrl, KnownFolders.MusicLibrary).Result;
                }
                else
                {
                    if (StorageHelper.FileExistsAsync(string.Format("songs/{0}.mp3", track.SongId)).Result)
                        file = StorageHelper.GetFileAsync(string.Format("songs/{0}.mp3", track.SongId)).Result;
                }

                if (file != null)
                {
                    try
                    {
                        _mediaPlayer.SetFileSource(file);
                    }
                    catch
                    {
                        if (!isLocal)
                        {
                            //corrupt download, perhaps
                            track.Song.SongState = SongState.None;
                            _sql.UpdateItem(track.Song);
                            file.DeleteAsync().AsTask().Wait();
                        }
                    }
                }
                else if (CurrentTrack.NextId != 0 && CurrentTrack.PrevId != 0)
                    SkipToNext();
            }
        }

        /// <summary>
        ///     Play all tracks in the list starting with 0
        /// </summary>
        public void PlayAllTracks()
        {
            StartTrack(GetQueueSongWherePrevId(0));
        }

        /// <summary>
        ///     Skip to next track
        /// </summary>
        public void SkipToNext()
        {
            var next = GetQueueSongWherePrevId(GetCurrentId()) ?? GetQueueSongWherePrevId(0);
            StartTrack(next);
        }

        private async void ScrobbleOnMediaEnded(QueueSong queue)
        {
            var item = GetHistoryItem(queue);
            if (item == null) return;

            item.Song = queue.Song;
            try
            {
                if (_scrobbler.CanScrobble(item.Song, _mediaPlayer.Position))
                {
                    if (await _scrobbler.Scrobble(item, _mediaPlayer.Position))
                    {
                        queue.Song.PlayCount++;
                        queue.Song.LastPlayed = item.DatePlayed;

                        if (queue.Song.Duration.Ticks != _mediaPlayer.NaturalDuration.Ticks)
                            queue.Song.Duration = _mediaPlayer.NaturalDuration;

                        _sql.UpdateItem(queue.Song);
                    }
                }
            }
            catch { }
            await _bgSql.DeleteItemAsync(item);
        }

        private HistoryEntry GetHistoryItem(QueueSong queue)
        {
            var history = _bgSql.SelectAll<HistoryEntry>();

            //if null then the player has just been launched
            if (queue == null)
            {
                //reset the incrementable Id of the table
                _bgSql.DeleteTableAsync<HistoryEntry>().Wait();
                return null;
            }

            return history.FirstOrDefault(p => p.SongId == queue.SongId);
        }

        /// <summary>
        ///     Skip to next track
        /// </summary>
        public void SkipToPrevious()
        {
            var prev = GetQueueSongWhereNextId(GetCurrentId()) ?? GetQueueSongWhereNextId(0);
            StartTrack(prev);
        }

        #endregion

        public void Dispose()
        {
            _bgSql.Dispose();
            _sql.Dispose();
        }

        private long GetCurrentId()
        {
            return AppSettingsHelper.Read<long>(PlayerConstants.CurrentTrack);
        }

        public QueueSong GetCurrentQueueSong()
        {
            return GetQueueSongById(GetCurrentId());
        }

        private QueueSong GetQueueSong(string prop, long id)
        {
            var queue = _bgSql.SelectWhere<QueueSong>(prop, id.ToString());
            if (queue != null)
            {
                var song = _sql.SelectWhere<Song>("Id", queue.SongId.ToString());
                var artist = _sql.SelectWhere<Artist>("Id", song.ArtistId.ToString());

                song.Artist = artist;
                queue.Song = song;
                return queue;
            }

            return null;
        }

        private bool IsShuffle { get { return AppSettingsHelper.Read<bool>("Shuffle"); } }

        public QueueSong GetQueueSongById(long id)
        {
            return GetQueueSong("Id", id);
        }

        public QueueSong GetQueueSongWhereNextId(long id)
        {
            return GetQueueSong((IsShuffle ? "Shuffle" : "") + "NextId", id);
        }

        public QueueSong GetQueueSongWherePrevId(long id)
        {
            return GetQueueSong((IsShuffle ? "Shuffle" : "") + "PrevId", id);
        }
    }
}