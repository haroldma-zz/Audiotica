using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Data.Collection.Model;

namespace Audiotica.Android.Utilities
{
    public static class AudioPlayerHelper
    {
        public static QueueSong CurrentQueueSong
        {
            get
            {
                return
                    App.Current.Locator.CollectionService.PlaybackQueue.FirstOrDefault(
                        p => p.Id == SettingsHelper.CurrentQueueId);
            }
        }

        public static async void PlaySong(QueueSong queueSong)
        {
            if (queueSong == null)
                return;

//            Insights.Track("Play Song", new Dictionary<string, string>
//            {
//                {"Name",queueSong.Song.Name},
//                {"ArtistName",queueSong.Song.ArtistName},
//                {"ProviderId",queueSong.Song.ProviderId}
//            });

            if (!App.Current.AudioServiceConnection.IsPlayerBound)
            {
                if (!await App.Current.StartPlaybackServiceAsync())
                    return;
            }

            App.Current.AudioServiceConnection.GetPlaybackService().PlaySong(queueSong);
        }
    }
}