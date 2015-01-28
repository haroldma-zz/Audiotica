#region

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Data.Model;
using Audiotica.Data.Service.RunTime;

#endregion

namespace Audiotica.Data
{
    public enum Mp3Provider
    {
        Mp3Truck,
        SoundCloud,
        Netease,
        Mp3Clan,
        Meile,
        YouTube,
        Mp3Skull,
    }

    public class Mp3MatchEngine
    {
        private readonly Mp3SearchService _service;

        private readonly Mp3Provider[] _providers =
        {
            Mp3Provider.Mp3Truck,
            Mp3Provider.Mp3Clan,
            Mp3Provider.Mp3Skull,
            Mp3Provider.Netease,
            Mp3Provider.Meile,
            Mp3Provider.SoundCloud,
           // Mp3Provider.YouTube <- links expire, not good for streaming
        };

        public Mp3MatchEngine(IAppSettingsHelper settingsHelper)
        {
            _service = new Mp3SearchService(settingsHelper);
        }

        public async Task<string> FindMp3For(string title, string artist)
        {
            title = title.ToLower()
                .Replace("feat.", "ft.") //better alternatives for matching
                .Replace("- bonus track", "")
                .Replace("bonus track", "")
                .Replace("- live", "(live)")
                .Replace("- remix", "(remix)")
                .Replace("a cappella", "acappella")
                .Replace("- acoustic version", "(acoustic)")
                .Replace("- acoustic", "(acoustic)")
                .Replace("- cover", "(cover)")
                .Replace("- stereo", "")
                .Replace("- mono", "")
                .Replace("- intro", "")
                .Replace("- no intro", "")
                .Replace("- ep version", "")
                .Replace("- deluxe edition", "").Trim();

            var currentProvider = 0;
            string url = null;

            while (currentProvider < _providers.Length)
            {
                var mp3Provider = _providers[currentProvider];
                url = await GetMatch(mp3Provider, title, artist).ConfigureAwait(false);

                if (url != null)
                    break;
                currentProvider++;
            }
            return url;
        }

        public async Task<string> GetMatch(Mp3Provider provider, string title, string artist,
            string album = null)
        {
            var webSongs = new List<WebSong>();

            switch (provider)
            {
                case Mp3Provider.Netease:
                    webSongs = await _service.SearchNetease(title, artist, album).ConfigureAwait(false);
                    break;
                case Mp3Provider.YouTube:
                    webSongs = await _service.SearchYoutube(title, artist, album).ConfigureAwait(false);
                    break;
                case Mp3Provider.Mp3Clan:
                    webSongs = await _service.SearchMp3Clan(title, artist, album).ConfigureAwait(false);
                    break;
                case Mp3Provider.Meile:
                    webSongs = await _service.SearchMeile(title, artist, album).ConfigureAwait(false);
                    break;
                case Mp3Provider.Mp3Truck:
                    webSongs = await _service.SearchMp3Truck(title, artist, album).ConfigureAwait(false);
                    break;
                case Mp3Provider.SoundCloud:
                    webSongs = await _service.SearchSoundCloud(title, artist, album).ConfigureAwait(false);
                    break;
                case Mp3Provider.Mp3Skull:
                    webSongs = await _service.SearchMp3Skull(title, artist, album).ConfigureAwait(false);
                    break;
            }

            return webSongs != null && webSongs.Any() ? webSongs.FirstOrDefault().AudioUrl : null;
        }
    }
}