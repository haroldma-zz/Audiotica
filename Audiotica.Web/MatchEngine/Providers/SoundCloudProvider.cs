using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Extensions;
using Audiotica.Core.Interfaces.Utilities;
using Audiotica.Web.Http.Requets;
using Audiotica.Web.Interfaces.MatchEngine;
using Audiotica.Web.Interfaces.MatchEngine.Validators;
using Audiotica.Web.Models;

namespace Audiotica.Web.MatchEngine.Providers
{
    public class SoundCloudProvider : ProviderBase
    {
        public SoundCloudProvider(IEnumerable<ISongTypeValidator> validators, ISettingsUtility settingsUtility)
            : base(validators, settingsUtility)
        {
        }

        public override string DisplayName => "SoundCloud";
        public override ProviderSpeed Speed => ProviderSpeed.Fast;
        public override ProviderResultsQuality ResultsQuality => ProviderResultsQuality.BetterThanNothing;

        protected override async Task<List<WebSong>> InternalGetSongsAsync(string title, string artist, int limit = 10)
        {
            using (var response = await new SoundCloudSearchRequest(ApiKeys.SoundCloudId, title.Append(artist))
                .Limit(limit).ToResponseAsync().DontMarshall())
            {
                return response.Data?.Collection?.Select(p => new WebSong(p))?.ToList();
            }
        }
    }
}