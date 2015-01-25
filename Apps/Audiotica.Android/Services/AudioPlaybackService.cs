using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Util;
using Audiotica.Android.Implementations;
using Audiotica.Core;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.RunTime;
using Uri = Android.Net.Uri;

namespace Audiotica.Android.Services
{
    [Service]
    public class AudioPlaybackService : Service, MediaPlayer.IOnPreparedListener, MediaPlayer.IOnErrorListener,
        MediaPlayer.IOnCompletionListener
    {
        public class AudioPlaybackBinder : Binder
        {
            private readonly AudioPlaybackService _service;

            public AudioPlaybackBinder(AudioPlaybackService service)
            {
                _service = service;
            }

            public AudioPlaybackService GetPlaybackService()
            {
                return _service;
            }
        }

        private IAppSettingsHelper _appSettingsHelper;
        //media player
        private MediaPlayer _player;
        private AudioPlaybackBinder _binder;

        private bool IsShuffle
        {
            get { return _appSettingsHelper.Read<bool>("Shuffle"); }
        }

        public const string PlayPauseAction = "audiotica.android.PlayPauseAction";
        public const string NextAction = "audiotica.android.NextAction";
        public const string PrevAction = "audiotica.android.PrevAction";

        #region Overrides and implementations

        public void OnCompletion(MediaPlayer mp)
        {
            SkipToNext();
        }

        public bool OnError(MediaPlayer mp, MediaError what, int extra)
        {
            Log.Error("AudioPlaybackService", "Playback error", what);
            return false;
        }

        public void OnPrepared(MediaPlayer mp)
        {
            mp.Start();
        }

        public void InitMusicPlayer()
        {
            _player.SetWakeMode(ApplicationContext, WakeLockFlags.Partial);
            _player.SetAudioStreamType(Stream.Music);
            _player.SetOnPreparedListener(this);
            _player.SetOnCompletionListener(this);
            _player.SetOnErrorListener(this);
        }

        public override void OnCreate()
        {
            base.OnCreate();
            _appSettingsHelper = new AppSettingsHelper();
            //create player
            _player = new MediaPlayer();

            InitMusicPlayer();
        }

        public override IBinder OnBind(Intent intent)
        {
            _binder = new AudioPlaybackBinder(this);
            return _binder;
        }

        public override bool OnUnbind(Intent intent)
        {
            _player.Stop();
            _player.Release();
            return false;
        }

        #endregion

        public async void SkipToNext()
        {
            var next = await GetQueueSongWherePrevIdAsync(GetCurrentId()) ?? await GetQueueSongWherePrevIdAsync(0);
            PlaySong(next);
        }

        public async void PlayPrev()
        {
            var prev = await GetQueueSongWhereNextIdAsync(GetCurrentId()) ?? await GetQueueSongWhereNextIdAsync(0);
            PlaySong(prev);
        }

        private bool _shouldReset;
        public async void PlaySong(QueueSong queue)
        {
            if (_shouldReset)
                _player.Reset();
            _shouldReset = true;
            _appSettingsHelper.Write(PlayerConstants.CurrentTrack, queue.Id);

            try
            {
                await _player.SetDataSourceAsync(ApplicationContext, Uri.Parse(queue.Song.AudioUrl));
            }
            catch (Exception e)
            {
                Log.Error("MUSIC SERVICE", "Error setting data source", e);
            }

            _player.PrepareAsync();
        }

        #region Helper methods

        private ISqlService CreatePlayerSqlService()
        {
            var factory = new AudioticaFactory(null, null, null);
            var sql = factory.CreatePlayerSqlService(4);
            sql.Initialize(readOnlyMode: true);
            return sql;
        }

        private ISqlService CreateCollectionSqlService()
        {
            var factory = new AudioticaFactory(null, null, null);
            var sql = factory.CreateCollectionSqlService(9);
            sql.Initialize();
            return sql;
        }

        private int GetCurrentId()
        {
            return _appSettingsHelper.Read<int>(PlayerConstants.CurrentTrack);
        }

        public Task<QueueSong> GetCurrentQueueSongAsync()
        {
            return GetQueueSongByIdAsync(GetCurrentId());
        }

        public Task<QueueSong> GetQueueSongByIdAsync(int id)
        {
            return GetQueueSongAsyncAsync(p => p.Id == id);
        }

        public Task<QueueSong> GetQueueSongWhereNextIdAsync(int id)
        {
            return IsShuffle ? GetQueueSongAsyncAsync(p => p.ShuffleNextId == id) : GetQueueSongAsyncAsync(p => p.NextId == id);
        }

        public Task<QueueSong> GetQueueSongWherePrevIdAsync(int id)
        {
            return IsShuffle ? GetQueueSongAsyncAsync(p => p.ShufflePrevId == id) : GetQueueSongAsyncAsync(p => p.PrevId == id);
        }

        private async Task<QueueSong> GetQueueSongAsyncAsync(Func<QueueSong, bool> expression)
        {
            using (var bgSql = CreatePlayerSqlService())
            {
                var queue = await bgSql.SelectFirstAsync(expression);

                if (queue == null) return null;
                using (var sql = CreateCollectionSqlService())
                {
                    var song = await sql.SelectFirstAsync<Song>(p => p.Id == queue.SongId);
                    var artist = await sql.SelectFirstAsync<Artist>(p => p.Id == song.ArtistId);
                    var album = await sql.SelectFirstAsync<Album>(p => p.Id == song.AlbumId);

                    song.Artist = artist;
                    song.Album = album;
                    song.Album.PrimaryArtist = artist;
                    queue.Song = song;
                    return queue;
                }
            }
        }

        #endregion
    }
}