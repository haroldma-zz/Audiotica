using Android.App;
using Android.Content;
using Audiotica.Android.Services;

namespace Audiotica.Android.BroadcastReceivers
{
    [BroadcastReceiver]
    [IntentFilter(new[]
    {
        AudioPlaybackService.PlayPauseAction,
        AudioPlaybackService.NextAction,
        AudioPlaybackService.PrevAction,
        AudioPlaybackService.StopAction,
        AudioPlaybackService.LaunchNowPlayingAction
    })]
    public class MediaActionReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var conn = App.Current.AudioServiceConnection;

            if (!conn.IsPlayerBound)
                return;

            var service = conn.GetPlaybackService();

            switch (intent.Action)
            {
                case AudioPlaybackService.PlayPauseAction:
                    service.TogglePlayPause();
                    break;
                case AudioPlaybackService.NextAction:
                    service.SkipToNext();
                    break;
                case AudioPlaybackService.PrevAction:
                    service.SkipToPrevious();
                    break;
                case AudioPlaybackService.StopAction:
                    App.Current.StopPlaybackService();
                    break;
            }
        }
    }
}