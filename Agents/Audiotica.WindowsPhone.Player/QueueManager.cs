#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Notifications;
using Audiotica.Core;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Core.WinRt;
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

        private bool IsRadioMode
        {
            get { return _appSettingsHelper.Read<bool>("RadioMode"); }
        }

        private int RadioId
        {
            get { return _appSettingsHelper.Read<int>("RadioId"); }
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

        protected virtual void OnEvent(EventHandler handler)
        {
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #region Private members

        private readonly IAppSettingsHelper _appSettingsHelper;
        private readonly SystemMediaTransportControls _transportControls;

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

        public QueueManager(IAppSettingsHelper appSettingsHelper, SystemMediaTransportControls transportControls)
        {
            _appSettingsHelper = appSettingsHelper;
            _transportControls = transportControls;
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
        public event TypedEventHandler<QueueManager, object>  TrackChanged;

        #endregion

        #region MediaPlayer Handlers

        private ScrobblerHelper _scrobbler;
        private int _retryCount;
        private FlacMediaSourceAdapter _currentMediaSourceAdapter;
        private RadioStation _station;

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

            if (TrackChanged != null)
                OnTrackChanged(CurrentTrack.SongId);

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

            UpdateTile();

            if (_scrobbler.IsScrobblingEnabled())
                await _scrobbler.UpdateNowPlaying(CurrentTrack);
        }

        public void UpdateTile()
        {
            var title = CurrentTrack.Song.Name;
            var artist = CurrentTrack.Song.Artist.Name;
            var album = CurrentTrack.Song.Album.Name;

            var imageUrl = CurrentTrack.Song.Album.HasArtwork
                ? AppConstant.LocalStorageAppPath +
                  string.Format(AppConstant.ArtworkPath, CurrentTrack.Song.Album.Id)
                : AppConstant.MissingArtworkAppPath;
            var wideImageUrl = CurrentTrack.Song.Artist.HasArtwork
                ? AppConstant.LocalStorageAppPath +
                  string.Format(AppConstant.ArtistsArtworkPath, CurrentTrack.Song.Artist.Id)
                : AppConstant.MissingArtworkAppPath;

            var expire = Math.Max(_mediaPlayer.NaturalDuration.TotalHours + .5, 1);

            var updater = TileUpdateManager.CreateTileUpdaterForApplication("App");
            updater.Update(GetSquareTile(title, artist, imageUrl, expire));
            updater.Update(GetWideTile(title, artist, album, wideImageUrl, expire));
        }

        private TileNotification GetSquareTile(string title, string artist, string imageUrl, double expire)
        {
            var tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150PeekImageAndText02);

            var tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = "Now Playing";
            tileTextAttributes[1].InnerText = title + "\nby " + artist;

            var tileImageAttributes = tileXml.GetElementsByTagName("image");
            ((XmlElement) tileImageAttributes[0]).SetAttribute("src", imageUrl);

            return new TileNotification(tileXml)
            {
                ExpirationTime = DateTimeOffset.UtcNow.AddHours(expire)
            };
        }

        private TileNotification GetWideTile(string title, string artist, string album, string imageUrl, double expire)
        {
            var tileWideXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150PeekImage02);

            var tileWideTextAttributes = tileWideXml.GetElementsByTagName("text");
            tileWideTextAttributes[0].InnerText = "Now Playing";
            tileWideTextAttributes[1].InnerText = title;
            tileWideTextAttributes[2].InnerText = "by " + artist;
            tileWideTextAttributes[3].InnerText = "on " + album;

            var tileWideImageAttributes = tileWideXml.GetElementsByTagName("image");
            ((XmlElement) tileWideImageAttributes[0]).SetAttribute("src", imageUrl);

            return new TileNotification(tileWideXml)
            {
                ExpirationTime = DateTimeOffset.UtcNow.AddHours(expire)
            };
        }

        /// <summary>
        ///     Handler for MediaPlayer Media Ended
        /// </summary>
        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            if (_appSettingsHelper.Read<bool>("Repeat"))
                InternalStartTrack(GetCurrentQueueSong());

            else
                SkipToNext(true);
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

        public async void StartRadioStation()
        {
            using (var service = CreateCollectionSqlService())
            {
                _station = await service.SelectFirstAsync<RadioStation>(p => p.Id == RadioId);

                if (_station == null) return;

                // Create the station manager
                var radioStationManager = new RadioStationManager(_station.GracenoteId, _station.Id,
                    CreateCollectionSqlService,
                    CreatePlayerSqlService);

                // Load tracks and add them to the database
                await radioStationManager.LoadTracksAsync();

                await radioStationManager.UpdateQueueAsync();

                OnEvent(QueueUpdated);

                _appSettingsHelper.Write(PlayerConstants.CurrentTrack, radioStationManager.QueueSongs[0].Id);
                StartRadioTrack(radioStationManager.QueueSongs[0]);
            }
        }

        public event EventHandler QueueUpdated;
        public event EventHandler MatchingTrack;

        public async void StartRadioTrack(QueueSong track)
        {
            if (track == null) return;

            switch (track.Song.SongState)
            {
                case SongState.BackgroundMatching:
                    _transportControls.IsPlayEnabled = false;
                    _transportControls.IsNextEnabled = false;
                    _transportControls.IsPreviousEnabled = false;
                    OnEvent(MatchingTrack);

                    // Create the station manager
                    var radioStationManager = new RadioStationManager(_station.GracenoteId, _station.Id,
                        CreateCollectionSqlService,
                        CreatePlayerSqlService);

                    var matched = await radioStationManager.MatchSongAsync(track.Song);
                    _transportControls.IsNextEnabled = true;
                    
                    if (!matched)
                    {
                        SkipToNext();
                        return;
                    }
                    break;
                case SongState.NoMatch:
                    SkipToNext();
                    break;
            }

            InternalStartTrack(track);
        }

        public async void StartTrack(QueueSong track, bool ended  = false)
        {
            if (track == null) return;

            ScrobbleOnMediaEnded();

            if (IsRadioMode && _station != null && _currentTrack != null && _currentTrack.Song.ProviderId.Contains("gn."))
            {
                // Create the station manager
                var radioStationManager = new RadioStationManager(_station.GracenoteId, _station.Id,
                    CreateCollectionSqlService,
                    CreatePlayerSqlService);

                var trackId = _currentTrack.Song.ProviderId.Replace("gn.", "");

                if (!ended)
                    await radioStationManager.SkippedAsync(trackId);
                else
                    await radioStationManager.PlayedAsync(trackId);

                await radioStationManager.UpdateQueueAsync();
                track = radioStationManager.QueueSongs[0];
                _appSettingsHelper.Write(PlayerConstants.CurrentTrack, track.Id);
                OnEvent(QueueUpdated);
            }

            _currentTrack = track;

            if (TrackChanged != null)
                OnTrackChanged(_currentTrack.SongId);

            _mediaPlayer.Pause();

            if (IsRadioMode)
                StartRadioTrack(track);
            else
                InternalStartTrack(track);
        }
        private async void InternalStartTrack(QueueSong track)
        {
            _transportControls.IsPreviousEnabled = !IsRadioMode;

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
        public void SkipToNext(bool ended = false)
        {
            var next = GetQueueSongWherePrevId(GetCurrentId()) ?? GetQueueSongWherePrevId(0);
            StartTrack(next, ended);
        }

        public void UpdateScrobblerInstance()
        {
            _scrobbler = new ScrobblerHelper(_appSettingsHelper, new ScrobblerService(new PclCredentialHelper()));
        }

        private async void ScrobbleOnMediaEnded()
        {
            var item = GetHistoryItem();
            if (item == null) return;

            try
            {
                if (!_scrobbler.CanScrobble(item.Song, _mediaPlayer.Position)) return;

                if (_scrobbler.IsScrobblingEnabled())
                {
                    await _scrobbler.Scrobble(item, _mediaPlayer.Position);
                }
                item.Song.PlayCount++;
                item.Song.LastPlayed = item.DatePlayed;

                if (item.Song.Duration.Ticks != _mediaPlayer.NaturalDuration.Ticks)
                    item.Song.Duration = _mediaPlayer.NaturalDuration;

                using (var sql = CreateCollectionSqlService())
                {
                    sql.UpdateItem(item.Song);
                }
            }
            catch
            {
            }
        }

        public async void FlushHistory()
        {
            using (var historySql = CreateHistorySqlService())
            {
                await historySql.DeleteTableAsync<HistoryEntry>();
            }
        }

        private HistoryEntry GetHistoryItem()
        {
            using (var historySql = CreateHistorySqlService())
            {
                var history = historySql.SelectAll<HistoryEntry>();
                var ret = history.FirstOrDefault();
                historySql.DeleteTableAsync<HistoryEntry>().Wait();

                if (ret == null) return null;

                ret.Song = GetQueueSong(p => p.SongId == ret.SongId).Song;
                return ret.Song == null ? null : ret;
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

        protected virtual void OnTrackChanged(object args)
        {
            var handler = TrackChanged;
            if (handler != null) handler(this, args);
        }
    }
}