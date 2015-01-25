using Android.Content;
using Android.Views;
using Audiotica.Android.Services;

namespace Audiotica.Android.BroadcastReceivers
{
    public class HeadsetButtonsReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            //There's no point in going any further if the service isn't running.
            if (!App.Current.AudioServiceConnection.IsPlayerBound) return;

            //Aaaaand there's no point in continuing if the intent doesn't contain info about headset control inputs.
            var intentAction = intent.Action;
            if (!Intent.ActionMediaButton.Equals(intentAction))
            {
                return;
            }

            var keyEvent = (KeyEvent) intent.GetParcelableExtra(Intent.ExtraKeyEvent);
            if (keyEvent.Action != KeyEventActions.Down) return;

            var keycode = keyEvent.KeyCode;

            //Switch through each event and perform the appropriate action based on the intent that's ben
            if (keycode == Keycode.MediaPlayPause || keycode == Keycode.Headsethook)
            {
                //Toggle play/pause.
                var playPauseIntent = new Intent();
                playPauseIntent.SetAction(AudioPlaybackService.PlayPauseAction);
                context.SendBroadcast(playPauseIntent);
            }

            if (keycode == Keycode.MediaNext)
            {
                //Fire a broadcast that skips to the next track.
                var nextIntent = new Intent();
                nextIntent.SetAction(AudioPlaybackService.NextAction);
                context.SendBroadcast(nextIntent);
            }

            if (keycode == Keycode.MediaPrevious)
            {
                //Fire a broadcast that goes back to the previous track.
                var previousIntent = new Intent();
                previousIntent.SetAction(AudioPlaybackService.PrevAction);
                context.SendBroadcast(previousIntent);
            }
        }
    }
}