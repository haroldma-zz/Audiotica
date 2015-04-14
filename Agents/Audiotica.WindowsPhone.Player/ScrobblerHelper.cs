using System;
using System.Threading.Tasks;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Service.Interfaces;
using IF.Lastfm.Core.Api.Enums;

namespace Audiotica.WindowsPhone.Player
{
    internal class ScrobblerHelper
    {
        private readonly IAppSettingsHelper _appSettingsHelper;
        private readonly IScrobblerService _service;

        public ScrobblerHelper(IAppSettingsHelper appSettingsHelper, IScrobblerService scrobblerService)
        {
            _appSettingsHelper = appSettingsHelper;
            _service = scrobblerService;
        }

        public bool IsScrobblingEnabled()
        {
            return _service.HasCredentials
                   && _appSettingsHelper.Read<bool>("Scrobble", SettingsStrategy.Roaming);
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

            return duration >= 30 && (playbackTime >= duration/2 || playbackTime >= 60*4);
        }

        public async Task UpdateNowPlaying(QueueSong queue)
        {
            try
            {
                await _service.ScrobbleNowPlayingAsync(queue.Song.Name, queue.Song.Artist.Name,
                    DateTime.UtcNow, queue.Song.Duration);
            }
            catch
            {
            }
        }

        public async Task<bool> Scrobble(HistoryEntry item, TimeSpan position)
        {
            var result =
                await
                    _service.ScrobbleAsync(item.Song.Name, item.Song.Artist.Name,
                        item.DatePlayed.ToUniversalTime(), item.Song.Duration);

            return result == LastResponseStatus.Successful || result == LastResponseStatus.Failure;
        }
    }
}