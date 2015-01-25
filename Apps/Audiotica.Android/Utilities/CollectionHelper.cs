using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Audiotica.Data.Collection.Model;

namespace Audiotica.Android.Utilities
{
    public static class CollectionHelper
    {
        private static bool _currentlyPreparing;
        private const int MaxMassPlayQueueCount = 100;
        public const double MaxPlayQueueCount = 2000;

        public static async Task PlaySongsAsync(List<Song> songs, bool random = false, bool forceClear = false)
        {
            if (songs.Count == 0) return;

            var index = random ? (songs.Count == 1 ? 0 : new Random().Next(0, songs.Count - 1)) : 0;
            var song = songs[index];

            await PlaySongsAsync(song, songs, forceClear);
        }

        public static async Task PlaySongsAsync(Song song, List<Song> songs, bool forceClear = false)
        {
            if (song == null || songs == null || songs.Count == 0) return;

            var skip = songs.IndexOf(song);
            var ordered = songs.Skip(skip).ToList();
            ordered.AddRange(songs.Take(skip));

            var overflow = songs.Count - MaxMassPlayQueueCount;
            if (overflow > 0)
                for (var i = 0; i < overflow; i++)
                    ordered.Remove(ordered.LastOrDefault());

            var playbackQueue = App.Current.Locator.CollectionService.PlaybackQueue.ToList();

            var sameLength = _currentlyPreparing || songs.Count < playbackQueue.Count ||
                             playbackQueue.Count >= MaxMassPlayQueueCount;
            var containsSong = playbackQueue.FirstOrDefault(p => p.SongId == song.Id) != null;
            var createQueue = forceClear || (!sameLength
                                             || !containsSong);

            if (_currentlyPreparing && createQueue)
            {
                //cancel the previous
                _currentlyPreparing = false;

                //wait for it to stop
                await Task.Delay(50);
            }

            if (!createQueue)
            {
                AudioPlayerHelper.PlaySong(playbackQueue.First(p => p.SongId == song.Id));
            }

            else
            {
                //                using (Insights.TrackTime("Create Queue", "Count", ordered.Count.ToString()))
                //                {
                _currentlyPreparing = true;

                await App.Current.Locator.CollectionService.ClearQueueAsync().ConfigureAwait(false);
                var queueSong = await App.Current.Locator.CollectionService.AddToQueueAsync(song).ConfigureAwait(false);
                AudioPlayerHelper.PlaySong(queueSong);

                App.Current.Locator.SqlService.BeginTransaction();
                for (var index = 1; index < ordered.Count; index++)
                {
                    if (!_currentlyPreparing)
                        break;
                    var s = ordered[index];
                    await App.Current.Locator.CollectionService.AddToQueueAsync(s).ConfigureAwait(false);
                }
                App.Current.Locator.SqlService.Commit();

                _currentlyPreparing = false;
                // }
            }
        }
    }
}