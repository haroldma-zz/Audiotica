using System.Linq;
using Audiotica.Core.Common;
using Audiotica.Core.Extensions;
using Audiotica.Core.Windows.Services;
using Audiotica.Database.Models;
using Audiotica.Web.MatchEngine.Interfaces.Providers;
using Audiotica.Web.Models;
using Audiotica.Windows.Common;

namespace Audiotica.Windows.Services
{
    public interface IWindowsPlayerService
    {
        void Play(WebSong song);
    }

    public class WindowsPlayerService : IWindowsPlayerService
    {
        private readonly IBackgroundAudioService _backgroundAudioService;
        private readonly IMatchEngineService _matchEngineService;
        private readonly IConverter<WebSong, Track> _webSongConverter;

        public WindowsPlayerService(IBackgroundAudioService backgroundAudioService,
            IMatchEngineService matchEngineService, IConverter<WebSong, Track> webSongConverter)
        {
            _backgroundAudioService = backgroundAudioService;
            _matchEngineService = matchEngineService;
            _webSongConverter = webSongConverter;
        }

        public async void Play(WebSong song)
        {
            var track = song.PreviousConversion as Track;
            using (var blocker = new UiBlocker())
            {
                if (track == null)
                {
                    blocker.UpdateProgress("Getting data...");
                    track = await _webSongConverter.ConvertAsync(song, webSong => { song.SetFrom(webSong); });
                }

                if (track.AudioWebUri == null)
                {
                    blocker.UpdateProgress("Matching...");
                    track.AudioWebUri = await _matchEngineService.GetLinkAsync(track.Title, track.DisplayArtist);
                }
            }
            if (track.AudioWebUri == null)
                CurtainPrompt.ShowError("Problem matching the song, try saving and manual matching it.");
            else
            {
                var queue = _backgroundAudioService.PlaybackQueue
                    .FirstOrDefault(p => TrackComparer.AreEqual(track, p.Track))
                            ?? _backgroundAudioService.Add(track);
                _backgroundAudioService.Play(queue);
            }
        }
    }
}