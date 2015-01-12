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

    public static class Mp3MatchEngine
    {
        private static readonly Mp3Provider[] Providers =
        {
            Mp3Provider.Mp3Skull,
            Mp3Provider.Mp3Clan,
            Mp3Provider.Netease,
            Mp3Provider.Mp3Truck,
            Mp3Provider.Meile,
            Mp3Provider.SoundCloud,
           // Mp3Provider.YouTube
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
                .Replace("- bonus track", "")
                .Replace("- stereo", "")
                .Replace("- mono", "")
                .Replace("- intro", "")
                .Replace("- no intro", "")
                .Replace("- ep version", "")
                .Replace("- deluxe edition", "").Trim();

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

        public static async Task<string> GetMatch(Mp3Provider provider, string title, string artist,
            string album = null)
        {
            var service = new Mp3SearchService();
            var webSongs = new List<WebSong>();

            switch (provider)
            {
                case Mp3Provider.Netease:
                    webSongs = await service.SearchNetease(title, artist, album, 3).ConfigureAwait(false);
                    break;
                case Mp3Provider.YouTube:
                    webSongs = await service.SearchYoutube(title, artist, album, 3).ConfigureAwait(false);
                    break;
                case Mp3Provider.Mp3Clan:
                    webSongs = await service.SearchMp3Clan(title, artist, album, 3).ConfigureAwait(false);
                    break;
                case Mp3Provider.Meile:
                    webSongs = await service.SearchMeile(title, artist, album, 3).ConfigureAwait(false);
                    break;
                case Mp3Provider.Mp3Truck:
                    webSongs = await service.SearchMp3Truck(title, artist, album).ConfigureAwait(false);
                    break;
                case Mp3Provider.SoundCloud:
                    webSongs = await service.SearchSoundCloud(title, artist, album).ConfigureAwait(false);
                    break;
                case Mp3Provider.Mp3Skull:
                    webSongs = await service.SearchMp3Skull(title, artist, album).ConfigureAwait(false);
                    break;
            }

            return webSongs != null && webSongs.Any() ? webSongs.FirstOrDefault().AudioUrl : null;
        }
    }
}