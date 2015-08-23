using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Web.Enums;
using Audiotica.Web.Exceptions;
using Audiotica.Web.Http.Requets.Metadata.LyricsFind;

namespace Audiotica.Web.Metadata.Providers
{
    public class LyricsFindMetadataProvider : MetadataProviderLyricsOnlyBase
    {
        public LyricsFindMetadataProvider(ISettingsUtility settingsUtility) : base(settingsUtility)
        {
        }

        public override string DisplayName => "LyricsFind";
        public override ProviderSpeed Speed => ProviderSpeed.Fast;

        public override async Task<string> GetLyricAsync(string song, string artist)
        {
            using (var response = await new LyricsFindRequest(song, artist).ToResponseAsync().DontMarshall())
            {
                if (response.HasData) return response.Data.Track?.Lyrics;
                throw new ProviderException("Not found.");
            }
        }
    }
}