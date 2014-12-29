#region

using System.Threading.Tasks;
using Audiotica.Data.Mp3Providers;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.Data
{
    public static class Mp3MatchEngine
    {
        private static readonly IMp3Provider[] Providers =
        {
            new NeteaseProvider(),
            new MeileProvider(),
            new YouTubeProvider(),
            new Mp3ClanProvider(),
            new Mp3TruckProvider(),
            new SoundCloudProvider()
        };


        public static async Task<string> FindMp3For(string title, string artist)
        {
            title = title.ToLower()
                .Replace("feat.", "ft.") //better alternatives for matching
                .Replace("- live", "(live)")
                .Replace("- bonus track", "");

            var currentProvider = 0;
            while (currentProvider < Providers.Length)
            {
                var mp3Provider = Providers[currentProvider];

                var url = await mp3Provider.GetMatch(title, artist).ConfigureAwait(false);

                if (url != null)
                    return url;

                currentProvider++;
            }

            return null;
        }
    }
}