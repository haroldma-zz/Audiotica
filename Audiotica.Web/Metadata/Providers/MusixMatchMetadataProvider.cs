using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Web.Enums;
using Audiotica.Web.Exceptions;
using Audiotica.Web.Http.Requets.Metadata.Google;

namespace Audiotica.Web.Metadata.Providers
{
    public class MusixMatchMetadataProvider : MetadataProviderLyricsOnlyBase
    {
        public MusixMatchMetadataProvider(ISettingsUtility settingsUtility) : base(settingsUtility)
        {
        }

        public override string DisplayName { get; } = "MusixMatch";

        public override ProviderSpeed Speed { get; } = ProviderSpeed.Fast;

        public override async Task<string> GetLyricAsync(string song, string artist)
        {
            using (var response = await new MusixMatchLyricsRequest(song, artist).ToResponseAsync().DontMarshall())
            {
                if (response.HasData)
                {
                    return response.Data["message"]["body"]["macro_calls"]["track.lyrics.get"]["message"]["body"]["lyrics"].Value<string>("lyrics_body");
                }
                throw new ProviderException("Not found.");
            }
        }
    }
}