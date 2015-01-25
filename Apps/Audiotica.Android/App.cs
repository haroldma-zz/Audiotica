using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Audiotica.Android.Services;
using Object = Java.Lang.Object;

namespace Audiotica.Android
{
    public class App : Application
    {
        public AudioPlaybackServiceConnection AudioServiceConnection = new AudioPlaybackServiceConnection();
        private Intent _playIntent;

        //mono needs this, else it... crashes
        public App(IntPtr a, JniHandleOwnership b) : base(a, b)
        {
        }

        public static App Current { get; set; }
        public Activity CurrentActivity { get; set; }
        public Locator Locator { get; set; }

        public override async void OnCreate()
        {
            base.OnCreate();
            Current = this;
            Locator = new Locator();

            await Current.Locator.SqlService.InitializeAsync();
            await Current.Locator.BgSqlService.InitializeAsync();
            await Current.Locator.CollectionService.LoadLibraryAsync();
        }

        private TaskCompletionSource<bool> _startPlaybackCompletionSource; 
        public Task<bool> StartPlaybackServiceAsync()
        {
            if (_playIntent != null) return Task.FromResult(false);

            _startPlaybackCompletionSource = new TaskCompletionSource<bool>();
            _playIntent = new Intent(this, typeof(AudioPlaybackService));
            BindService(_playIntent, AudioServiceConnection, Bind.AutoCreate);
            StartService(_playIntent);

            return _startPlaybackCompletionSource.Task;
        }

        public void StopPlaybackService()
        {
            AudioServiceConnection.StopService();
            _playIntent = null;
        }

        public class AudioPlaybackServiceConnection : Object, IServiceConnection
        {
            private AudioPlaybackService.AudioPlaybackBinder _audioPlaybackBinder;

            public void OnServiceConnected(ComponentName name, IBinder service)
            {
                var audioPlaybackBinder = (AudioPlaybackService.AudioPlaybackBinder) service;
                if (audioPlaybackBinder == null)
                {
                    Current._startPlaybackCompletionSource.SetResult(false);
                    return;
                }

                _audioPlaybackBinder = audioPlaybackBinder;
                IsPlayerBound = true;
                _audioPlaybackBinder.GetPlaybackService().InitMusicPlayer();
                Current._startPlaybackCompletionSource.SetResult(true);
            }

            public void OnServiceDisconnected(ComponentName name)
            {
                IsPlayerBound = false;

                if (_audioPlaybackBinder.IsBinderAlive)
                    _audioPlaybackBinder.Dispose();
                _audioPlaybackBinder = null;
            }


            public bool IsPlayerBound { get; set; }

            public AudioPlaybackService GetPlaybackService()
            {
                return _audioPlaybackBinder.GetPlaybackService();
            }

            public void StopService()
            {
                IsPlayerBound = false;
               
                try
                {
                    Context.UnbindService(this);
                    _audioPlaybackBinder.Dispose();
                    _audioPlaybackBinder = null;
                }
                catch
                {
                }

                Context.StopService(new Intent(Context, typeof (AudioPlaybackService)));
            }
        }
    }

    public enum RepeatMode
    {
        None,
        Song
    }
}