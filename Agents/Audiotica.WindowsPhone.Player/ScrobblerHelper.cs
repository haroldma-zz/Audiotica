using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Service.RunTime;
using IF.Lastfm.Core.Api.Enums;

namespace Audiotica.WindowsPhone.Player
{
    internal class ScrobblerHelper
    {
        private readonly ScrobblerService _service;

        public ScrobblerHelper()
        {
            _service = new ScrobblerService();
        }

        public bool IsScrobblingEnabled()
        {
            return _service.IsAuthenticated 
                && AppSettingsHelper.Read<bool>("Scrobble", SettingsStrategy.Roaming);
        }

        public bool CanScrobble(Song song, TimeSpan position)
        {
            /* When is a scrobble a scrobble?
                * A track should only be scrobbled when the following conditions have been met:
                * 1. The track must be longer than 30 seconds.
                * 2. And the track has been played for at least half its duration, or for 4 minutes (whichever occurs earlier.)
                */

            var playbackTime = position.TotalSeconds;
            var duration = song.Duration.TotalSeconds;

            return duration >= 30 && (playbackTime >= duration / 2 || playbackTime >= 60 * 4);
        }

        public async Task UpdateNowPlaying(QueueSong queue)
        {
            await _service.ScrobbleNowPlayingAsync(queue.Song.Name, queue.Song.ArtistName,
                    DateTime.UtcNow, queue.Song.Duration);
        }

        public async Task<bool> Scrobble(HistoryEntry item, TimeSpan position)
        {
            var result =
                await
                    _service.ScrobbleAsync(item.Song.Name, item.Song.ArtistName,
                        item.DatePlayed.ToUniversalTime(), item.Song.Duration);

            return result == LastFmApiError.None || result == LastFmApiError.Failure;
        }
    }
}
