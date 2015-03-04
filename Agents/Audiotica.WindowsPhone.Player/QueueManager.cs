#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.Storage;
using Audiotica.Core;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Core.WinRt.Utilities;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.RunTime;
using Audiotica.Data.Service.RunTime;
using Audiotica.Flac.WindowsPhone;

#endregion

namespace Audiotica.WindowsPhone.Player
{
    internal class QueueManager
    {
        private bool IsShuffle
        {
            get { return _appSettingsHelper.Read<bool>("Shuffle"); }
        }

        private int GetCurrentId()
        {
            return _appSettingsHelper.Read<int>(PlayerConstants.CurrentTrack);
        }

        public QueueSong GetCurrentQueueSong()
        {
            return GetQueueSongById(GetCurrentId());
        }

        private QueueSong GetQueueSong(Func<QueueSong, bool> expression)
        {
            using (var bgSql = CreatePlayerSqlService())
            {
                var queue = bgSql.SelectFirst(expression);

                if (queue == null) return null;
                using (var sql = CreateCollectionSqlService())
                {
                    var song = sql.SelectFirst<Song>(p => p.Id == queue.SongId);
                    var artist = sql.SelectFirst<Artist>(p => p.Id == song.ArtistId);
                    var album = sql.SelectFirst<Album>(p => p.Id == song.AlbumId);

                    song.Artist = artist;
                    song.Album = album;
                    song.Album.PrimaryArtist = artist;
                    queue.Song = song;
                    return queue;
                }
            }
        }

        public QueueSong GetQueueSongById(int id)
        {
            return GetQueueSong(p => p.Id == id);
        }

        public QueueSong GetQueueSongWhereNextId(int id)
        {
            return IsShuffle ? GetQueueSong(p => p.ShuffleNextId == id) : GetQueueSong(p => p.NextId == id);
        }

        public QueueSong GetQueueSongWherePrevId(int id)
        {
            return IsShuffle ? GetQueueSong(p => p.ShufflePrevId == id) : GetQueueSong(p => p.PrevId == id);
        }

        #region Private members

        private readonly IAppSettingsHelper _appSettingsHelper;

        private SqlService CreateHistorySqlService()
        {
            var historyDbTypes = new List<Type>
            {
                typeof (HistoryEntry)
            };
            var historyConfig = new SqlServiceConfig
            {
                Tables = historyDbTypes,
                CurrentVersion = 2,
                Path = "history.sqldb"
            };
            var sql = new SqlService(historyConfig);
            sql.Initialize();
            return sql;
        }

        private SqlService CreatePlayerSqlService()
        {
            var bgDbTypes = new List<Type>
            {
                typeof (QueueSong)
            };
            var bgConfig = new SqlServiceConfig
            {
                Tables = bgDbTypes,
                CurrentVersion = 6,
                Path = "player.sqldb"
            };
            var sql = new SqlService(bgConfig);
            sql.Initialize();
            return sql;
        }

        private SqlService CreateCollectionSqlService()
        {
            var dbTypes = new List<Type>
            {
                typeof (Artist),
                typeof (Album),
                typeof (Song),
                typeof (RadioStation),
                typeof (Playlist),
                typeof (PlaylistSong)
            };
            var config = new SqlServiceConfig
            {
                Tables = dbTypes,
                CurrentVersion = 11,
                Path = "collection.sqldb"
            };

            var sql = new SqlService(config);
            sql.Initialize();
            return sql;
        }

        private readonly MediaPlayer _mediaPlayer;
        private QueueSong _currentTrack;

        public QueueManager(IAppSettingsHelper appSettingsHelper)
        {
            _appSettingsHelper = appSettingsHelper;
            UpdateScrobblerInstance();

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
            get { return _currentTrack ?? (_currentTrack = GetCurrentQueueSong()); }
        }

        /// <summary>
        ///     Invoked when the media player is ready to move to next track
        /// </summary>
        public event TypedEventHandler<QueueManager, object> TrackChanged;

        #endregion

        #region MediaPlayer Handlers

        private ScrobblerHelper _scrobbler;
        private int _retryCount;
        private FlacMediaSourceAdapter _currentMediaSourceAdapter;

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

            using (var historySql = CreateHistorySqlService())
            {
                await historySql.InsertAsync(historyItem);
            }

            if (CurrentTrack.Song.Duration.Ticks != _mediaPlayer.NaturalDuration.Ticks)
            {
                CurrentTrack.Song.Duration = _mediaPlayer.NaturalDuration;

                using (var sql = CreateCollectionSqlService())
                {
                    await sql.UpdateItemAsync(CurrentTrack.Song);
                }
            }

            if (_scrobbler.IsScrobblingEnabled())
                await _scrobbler.UpdateNowPlaying(CurrentTrack);
        }

        /// <summary>
        ///     Handler for MediaPlayer Media Ended
        /// </summary>
        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            if (_appSettingsHelper.Read<bool>("Repeat"))
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

        #endregion

        #region Playlist command handlers

        public async void StartTrack(QueueSong track)
        {
            if (track == null)
                return;

            ScrobbleOnMediaEnded(_currentTrack);

            _currentTrack = track;
            _mediaPlayer.Pause();

            // If the flac media source adapter is not null, disposed of it
            // since we won't be using it

            if (_currentMediaSourceAdapter != null)
            {
                _currentMediaSourceAdapter.Dispose();
                _currentMediaSourceAdapter = null;
            }

            _mediaPlayer.AutoPlay = false;

            if (track.Song.IsStreaming)
            {
                _mediaPlayer.SetUriSource(new Uri(track.Song.AudioUrl));
            }
            else
            {
                var file = StorageFile.GetFileFromPathAsync(track.Song.AudioUrl).AsTask().Result;

                if (file != null)
                {
                    try
                    {
                        if (file.FileType != ".flac")
                        {
                            _mediaPlayer.SetFileSource(file);
                        }
                        else
                        {
                            // Use custom media source for FLAC support
                            _currentMediaSourceAdapter =
                                await FlacMediaSourceAdapter.CreateAsync(file);
                            BackgroundMediaPlayer.Current.SetMediaSource(_currentMediaSourceAdapter.MediaSource);
                        }
                    }
                    catch
                    {
                        SkipToNext();
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

        public void UpdateScrobblerInstance()
        {
            _scrobbler = new ScrobblerHelper(_appSettingsHelper, new ScrobblerService(new PclCredentialHelper()));
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
                    if (_scrobbler.IsScrobblingEnabled())
                    {
                        await _scrobbler.Scrobble(item, _mediaPlayer.Position);
                    }
                    queue.Song.PlayCount++;
                    queue.Song.LastPlayed = item.DatePlayed;

                    if (queue.Song.Duration.Ticks != _mediaPlayer.NaturalDuration.Ticks)
                        queue.Song.Duration = _mediaPlayer.NaturalDuration;

                    using (var sql = CreateCollectionSqlService())
                    {
                        sql.UpdateItem(queue.Song);
                    }
                }
            }
            catch
            {
            }

            using (var historySql = CreateHistorySqlService())
            {
                await historySql.DeleteItemAsync(item);
            }
        }

        private HistoryEntry GetHistoryItem(QueueSong queue)
        {
            using (var historySql = CreateHistorySqlService())
            {
                var history = historySql.SelectAll<HistoryEntry>();

                //if null then the player has just been launched
                if (queue == null)
                {
                    //reset the incrementable Id of the table
                    historySql.DeleteTableAsync<HistoryEntry>().Wait();
                    return null;
                }

                return history.FirstOrDefault(p => p.SongId == queue.SongId);
            }
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
    }
}