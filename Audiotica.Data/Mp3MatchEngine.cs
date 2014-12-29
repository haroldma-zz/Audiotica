#region

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Data.Model;
using Audiotica.Data.Service.RunTime;

#endregion

namespace Audiotica.Data
{
    public static class Mp3MatchEngine
    {
        public enum MatchProvider
        {
            Mp3Truck,
            SoundCloud,
            Netease,
            Mp3Clan,
            Meile,
            YouTube,
        }

        private static readonly MatchProvider[] Providers =
        {
            MatchProvider.Netease,
            MatchProvider.Mp3Clan,
            MatchProvider.Mp3Truck,
            MatchProvider.YouTube,
            MatchProvider.Meile,
            MatchProvider.SoundCloud,
        };


        public static async Task<string> FindMp3For(string title, string artist)
        {
            title = title.ToLower()
                .Replace("feat.", "ft.") //better alternatives for matching
                .Replace("- live", "(live)")
                .Replace("- remix", "(remix)")
                .Replace("a cappella", "acappella")
                .Replace("- acoustic version", "(acoustic)")
                .Replace("- cover", "(cover)")
                .Replace("- bonus track", "");

            var currentProvider = 0;
            string url = null;

            while (currentProvider < Providers.Length)
            {
                var mp3Provider = Providers[currentProvider];
                url = await GetMatch(mp3Provider, title, artist).ConfigureAwait(false);

                if (url != null)
                    break;
                currentProvider++;
            }
            return url;
        }

        public static async Task<string> GetMatch(MatchProvider provider, string title, string artist,
            string album = null)
        {
            var service = new Mp3SearchService();
            var webSongs = new List<WebSong>();

            switch (provider)
            {
                case MatchProvider.Netease:
                    webSongs = await service.SearchNetease(title, artist, album, 3).ConfigureAwait(false);
                    break;
                case MatchProvider.YouTube:
                    webSongs = await service.SearchYoutube(title, artist, album, 3).ConfigureAwait(false);
                    break;
                case MatchProvider.Mp3Clan:
                    webSongs = await service.SearchMp3Clan(title, artist, album, 3).ConfigureAwait(false);
                    break;
                case MatchProvider.Meile:
                    webSongs = await service.SearchMeile(title, artist, album, 3).ConfigureAwait(false);
                    break;
                case MatchProvider.Mp3Truck:
                    webSongs = await service.SearchMp3Truck(title, artist, album).ConfigureAwait(false);
                    break;
                case MatchProvider.SoundCloud:
                    webSongs = await service.SearchSoundCloud(title, artist, album, 3).ConfigureAwait(false);
                    break;
            }

            return webSongs != null && webSongs.Any() ? webSongs.FirstOrDefault().AudioUrl : null;
        }
    }
}