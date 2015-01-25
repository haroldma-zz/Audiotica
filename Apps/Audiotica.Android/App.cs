using System;
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
        public AudioServiceConnection AudioServiceConnection = new AudioServiceConnection();
        private Intent _playIntent;

        //mono needs this, else it... crashes
        public App(IntPtr a, JniHandleOwnership b) : base(a, b)
        {
        }

        public static App Current { get; set; }
        public Activity CurrentActivity { get; set; }
        public Locator Locator { get; set; }
        public double CrossfadeDuration { get; set; }
        public RepeatMode RepeatMode { get; set; }

        public override async void OnCreate()
        {
            base.OnCreate();
            Current = this;
            Locator = new Locator();

            await Current.Locator.SqlService.InitializeAsync();
            await Current.Locator.BgSqlService.InitializeAsync();
            await Current.Locator.CollectionService.LoadLibraryAsync();

            if (_playIntent != null) return;

            _playIntent = new Intent(this, Java.Lang.Class.FromType(typeof (AudioPlaybackService)));
            BindService(_playIntent, AudioServiceConnection, Bind.AutoCreate);
            StartService(_playIntent);
        }
    }

    public class AudioServiceConnection : Object, IServiceConnection
    {
        private AudioPlaybackService.AudioPlaybackBinder _audioPlaybackBinder;

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            var audioPlaybackBinder = (AudioPlaybackService.AudioPlaybackBinder) service;
            if (audioPlaybackBinder == null) return;

            _audioPlaybackBinder = audioPlaybackBinder;
            IsPlayerBound = true;

            while (!App.Current.Locator.CollectionService.IsLibraryLoaded)
            {
                
            }

            _audioPlaybackBinder.GetPlaybackService().PlaySong(App.Current.Locator.CollectionService.PlaybackQueue[0]);
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            IsPlayerBound = false;
        }


        public bool IsPlayerBound { get; set; }

        public AudioPlaybackService GetPlaybackService()
        {
            return _audioPlaybackBinder.GetPlaybackService();
        }
    }

    public enum RepeatMode
    {
        None,
        Song
    }
}