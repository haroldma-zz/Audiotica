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

        public static async Task<string> FindMp3For(LastTrack track)
        {
            //match engines get better results using ft instead of feat
            //so rename if it contains that
            var title = track.Name.Replace("feat.", "ft.");
            var artist = track.ArtistName;


            var currentProvider = 0;
            while (currentProvider < Providers.Length)
            {
                var mp3Provider = Providers[currentProvider];
                var url = await mp3Provider.GetMatch(title, artist);

                if (url != null)
                    return url;

                currentProvider++;
            }

            return null;
        }
    }
}