using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Audiotica.Android.BroadcastReceivers;
using Audiotica.Android.Implementations;
using Audiotica.Android.Utilities;
using Audiotica.Core;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using Java.Lang;
using Exception = System.Exception;
using Uri = Android.Net.Uri;

namespace Audiotica.Android.Services
{
    [Service]
    public class AudioPlaybackService : Service, MediaPlayer.IOnPreparedListener, MediaPlayer.IOnErrorListener,
        MediaPlayer.IOnCompletionListener
    {
        public const int NotificationId = 1080;
        //NOTE: Using 0 as a notification ID causes Android to ignore the notification call.

        //Custom actions for media player controls via the notification bar.
        public const string PlayPauseAction = "audiotica.android.PlayPauseAction";
        public const string NextAction = "audiotica.android.NextAction";
        public const string PrevAction = "audiotica.android.PrevAction";
        public const string StopAction = "audiotica.android.StopAction";
        public const string LaunchNowPlayingAction = "audiotica.android.LaunchNowPlayingAction";
        private bool _alreadyStarted;
        private AudioManager _audioManager;
        private AudioPlaybackBinder _binder;
        private ComponentName _mediaButtonReceiverComponent;
        private MediaPlayerState _mediaPlayerState;
        //Notification elements.
        private NotificationCompat.Builder _notificationBuilder;
        //media player
        private MediaPlayer _player;
        private WifiManager.WifiLock _wifiLock;
        private AppSettingsHelper _appSettingsHelper;

        public MediaPlayerState MediaPlayerState
        {
            get { return _mediaPlayerState; }
            private set
            {
                _mediaPlayerState = value;
                OnStateChanged(value);
            }
        }

        public bool IsPlayingMusic
        {
            get { return _player != null && _player.IsPlaying; }
        }

        private bool IsShuffle
        {
            get { return _appSettingsHelper.Read<bool>("Shuffle"); }
        }

        public event EventHandler TrackChanged;
        public event EventHandler MediaFailed;
        public event EventHandler<PlaybackStateEventArgs> StateChanged;

        public void TogglePlayPause()
        {
            if (IsPlayingMusic)
                PausePlayer();
            else
                PlayResumePlayer();
        }

        public void PausePlayer()
        {
            try
            {
                _player.Pause();
                MediaPlayerState = MediaPlayerState.Paused;
            }
            catch
            {
                MediaPlayerState = MediaPlayerState.None;
            }

            if (_wifiLock.IsHeld)
                _wifiLock.Release();
        }

        public void PlayResumePlayer()
        {
            if (MediaPlayerState == MediaPlayerState.Stopped || MediaPlayerState == MediaPlayerState.None)
            {
                PlaySong(AudioPlayerHelper.CurrentQueueSong);
                return;
            }

            try
            {
                if (!_wifiLock.IsHeld)
                    _wifiLock.Acquire();
                _player.Start();
                MediaPlayerState = MediaPlayerState.Playing;
            }
            catch
            {
                MediaPlayerState = MediaPlayerState.None;
            }
        }

        public void SkipToNext()
        {
            var next = GetQueueSongWherePrevId(SettingsHelper.CurrentQueueId) ?? GetQueueSongWherePrevId(0);
            PlaySong(next);
        }

        public void SkipToPrevious()
        {
            var prev = GetQueueSongWhereNextId(SettingsHelper.CurrentQueueId) ?? GetQueueSongWhereNextId(0);
            PlaySong(prev);
        }

        public async void PlaySong(QueueSong queue)
        {
            _appSettingsHelper.Write(PlayerConstants.CurrentTrack, queue.Id);
            if (_alreadyStarted)
                _player.Reset();
            else
            {
                StartForeground(NotificationId, BuildNotification());
            }
            _alreadyStarted = true;

            try
            {
                await _player.SetDataSourceAsync(ApplicationContext, Uri.Parse(queue.Song.AudioUrl));
                MediaPlayerState = MediaPlayerState.Buffering;
                _player.PrepareAsync();
            }
            catch (Exception e)
            {
                MediaPlayerState = MediaPlayerState.None;
                Log.Error("MUSIC SERVICE", "Player error", e);
            }
        }

        protected virtual void RaiseEvent(EventHandler handler)
        {
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected virtual void OnStateChanged(MediaPlayerState e)
        {
            if (e != MediaPlayerState.None && e != MediaPlayerState.Stopped)
                StartForeground(NotificationId, BuildNotification());
            else
                StopForeground(true);
            var handler = StateChanged;
            if (handler != null) handler(this, new PlaybackStateEventArgs(e));
        }

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

        #region Overrides and implementations

        public void OnCompletion(MediaPlayer mp)
        {
            SkipToNext();
        }

        public bool OnError(MediaPlayer mp, MediaError what, int extra)
        {
            Log.Error("AudioPlaybackService", "Playback error", what);
            RaiseEvent(MediaFailed);
            MediaPlayerState = MediaPlayerState.None;
            return false;
        }

        public void OnPrepared(MediaPlayer mp)
        {
            if (!_wifiLock.IsHeld)
                _wifiLock.Acquire();
            mp.Start();
            MediaPlayerState = MediaPlayerState.Playing;
            RaiseEvent(TrackChanged);
        }

        public void InitMusicPlayer()
        {
            _appSettingsHelper = new AppSettingsHelper();
            _player = new MediaPlayer();
            _wifiLock = ((WifiManager)GetSystemService(WifiService)).CreateWifiLock(WifiMode.Full, "playerLock");

            _player.SetWakeMode(ApplicationContext, WakeLockFlags.Partial);
            _player.SetAudioStreamType(Stream.Music);
            _player.SetOnPreparedListener(this);
            _player.SetOnCompletionListener(this);
            _player.SetOnErrorListener(this);

            _audioManager = (AudioManager) GetSystemService(AudioService);

            _mediaButtonReceiverComponent = new ComponentName(ApplicationContext,
                Class.FromType(typeof (HeadsetButtonsReceiver)));
            _audioManager.RegisterMediaButtonEventReceiver(_mediaButtonReceiverComponent);
        }

        public override IBinder OnBind(Intent intent)
        {
            _binder = new AudioPlaybackBinder(this);
            return _binder;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            MediaPlayerState = MediaPlayerState.Stopped;
            _alreadyStarted = false;

            if (_wifiLock.IsHeld)
                _wifiLock.Release();
            _player.Stop();
            _player.Release();
            _player = null;
        }

        #endregion

        #region Helper methods

        private Notification BuildNotification()
        {
            _notificationBuilder = new NotificationCompat.Builder(ApplicationContext);
            _notificationBuilder.SetOngoing(true);
            _notificationBuilder.SetAutoCancel(false);
            _notificationBuilder.SetSmallIcon(Resource.Drawable.NotifIcon);
            //Open up the player screen when the user taps on the notification.
            var launchNowPlayingIntent = new Intent();
            launchNowPlayingIntent.SetAction(LaunchNowPlayingAction);
            var launchNowPlayingPendingIntent = PendingIntent.GetBroadcast(ApplicationContext, 0, launchNowPlayingIntent,
                0);
            _notificationBuilder.SetContentIntent(launchNowPlayingPendingIntent);

            //Grab the notification layouts.
            var notificationView = new RemoteViews(ApplicationContext.PackageName,
                Resource.Layout.notification_custom_layout);
            var expNotificationView = new RemoteViews(ApplicationContext.PackageName,
                Resource.Layout.notification_custom_expanded_layout);

            //Initialize the notification layout buttons.
            var previousTrackIntent = new Intent();
            previousTrackIntent.SetAction(PrevAction);
            var previousTrackPendingIntent = PendingIntent.GetBroadcast(ApplicationContext, 0,
                previousTrackIntent, 0);

            var playPauseTrackIntent = new Intent();
            playPauseTrackIntent.SetAction(PlayPauseAction);
            var playPauseTrackPendingIntent = PendingIntent.GetBroadcast(ApplicationContext, 0,
                playPauseTrackIntent, 0);

            var nextTrackIntent = new Intent();
            nextTrackIntent.SetAction(NextAction);
            var nextTrackPendingIntent = PendingIntent.GetBroadcast(ApplicationContext, 0, nextTrackIntent, 0);

            var stopServiceIntent = new Intent();
            stopServiceIntent.SetAction(StopAction);
            var stopServicePendingIntent = PendingIntent.GetBroadcast(ApplicationContext, 0, stopServiceIntent,
                0);

            //Check if audio is playing and set the appropriate play/pause button.
            if (App.Current.AudioServiceConnection.GetPlaybackService().IsPlayingMusic)
            {
                notificationView.SetImageViewResource(Resource.Id.notification_base_play,
                    Resource.Drawable.btn_playback_pause_light);
                expNotificationView.SetImageViewResource(Resource.Id.notification_expanded_base_play,
                    Resource.Drawable.btn_playback_pause_light);
            }
            else
            {
                notificationView.SetImageViewResource(Resource.Id.notification_base_play,
                    Resource.Drawable.btn_playback_play_light);
                expNotificationView.SetImageViewResource(Resource.Id.notification_expanded_base_play,
                    Resource.Drawable.btn_playback_play_light);
            }

            var song = AudioPlayerHelper.CurrentQueueSong.Song;

            //Set the notification content.
            expNotificationView.SetTextViewText(Resource.Id.notification_expanded_base_line_one, song.Name);
            expNotificationView.SetTextViewText(Resource.Id.notification_expanded_base_line_two, song.ArtistName);
            expNotificationView.SetTextViewText(Resource.Id.notification_expanded_base_line_three, song.Album.Name);

            notificationView.SetTextViewText(Resource.Id.notification_base_line_one, song.Name);
            notificationView.SetTextViewText(Resource.Id.notification_base_line_two, song.ArtistName);

            //the previous and next buttons, always enabled.
            expNotificationView.SetViewVisibility(Resource.Id.notification_expanded_base_previous, ViewStates.Visible);
            expNotificationView.SetViewVisibility(Resource.Id.notification_expanded_base_next, ViewStates.Visible);
            expNotificationView.SetOnClickPendingIntent(Resource.Id.notification_expanded_base_play,
                playPauseTrackPendingIntent);
            expNotificationView.SetOnClickPendingIntent(Resource.Id.notification_expanded_base_next,
                nextTrackPendingIntent);
            expNotificationView.SetOnClickPendingIntent(Resource.Id.notification_expanded_base_previous,
                previousTrackPendingIntent);

            notificationView.SetViewVisibility(Resource.Id.notification_base_previous, ViewStates.Visible);
            notificationView.SetViewVisibility(Resource.Id.notification_base_next, ViewStates.Visible);
            notificationView.SetOnClickPendingIntent(Resource.Id.notification_base_play, playPauseTrackPendingIntent);
            notificationView.SetOnClickPendingIntent(Resource.Id.notification_base_next, nextTrackPendingIntent);
            notificationView.SetOnClickPendingIntent(Resource.Id.notification_base_previous, previousTrackPendingIntent);

            //Set the "Stop Service" pending intents.
            expNotificationView.SetOnClickPendingIntent(Resource.Id.notification_expanded_base_collapse,
                stopServicePendingIntent);
            notificationView.SetOnClickPendingIntent(Resource.Id.notification_base_collapse, stopServicePendingIntent);

            //Set the album art.
            if (song.Album.Artwork != null)
            {
                if (song.Album.Artwork.Image != null)
                {
                    expNotificationView.SetImageViewBitmap(Resource.Id.notification_expanded_base_image,
                        song.Album.Artwork.Image as Bitmap);
                    notificationView.SetImageViewBitmap(Resource.Id.notification_base_image,
                        song.Album.Artwork.Image as Bitmap);
                }
                else
                    ((PclBitmapImage) song.Album.Artwork).PropertyChanged += OnPropertyChanged;
            }
            else
                song.Album.PropertyChanged += OnPropertyChanged;

            //Attach the shrunken layout to the notification.
            _notificationBuilder.SetContent(notificationView);

            //Build the notification object.
            var notification = _notificationBuilder.Build();

            //Attach the expanded layout to the notification and set its flags.
            notification.BigContentView = expNotificationView;
            notification.Flags = NotificationFlags.ForegroundService |
                                 NotificationFlags.NoClear |
                                 NotificationFlags.OngoingEvent;
            return notification;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            ((INotifyPropertyChanged) sender).PropertyChanged -= OnPropertyChanged;
            StartForeground(NotificationId, BuildNotification());
        }

        private ISqlService CreatePlayerSqlService()
        {
            var factory = new AudioticaFactory(null, null, null);
            var sql = factory.CreatePlayerSqlService(4);
            sql.Initialize(readOnlyMode: true);
            return sql;
        }

        public QueueSong GetCurrentQueueSong()
        {
            return GetQueueSong(p => p.Id == SettingsHelper.CurrentQueueId);
        }

        public QueueSong GetQueueSongById(int id)
        {
            return GetQueueSong(p => p.Id == id);
        }

        public QueueSong GetQueueSongWhereNextId(int id)
        {
            return IsShuffle
                ? GetQueueSong(p => p.ShuffleNextId == id)
                : GetQueueSong(p => p.NextId == id);
        }

        public QueueSong GetQueueSongWherePrevId(int id)
        {
            return IsShuffle
                ? GetQueueSong(p => p.ShufflePrevId == id)
                : GetQueueSong(p => p.PrevId == id);
        }

        private QueueSong GetQueueSong(Func<QueueSong, bool> expression)
        {
            return App.Current.Locator.CollectionService.PlaybackQueue.FirstOrDefault(expression);
        }

        #endregion
    }

    public enum MediaPlayerState
    {
        None,
        Playing,
        Paused,
        Stopped,
        Buffering
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