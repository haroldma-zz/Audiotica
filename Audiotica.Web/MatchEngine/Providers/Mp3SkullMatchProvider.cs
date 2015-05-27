using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Audiotica.Core.Interfaces.Utilities;
using Audiotica.Web.Enums;
using Audiotica.Web.MatchEngine.Interfaces;
using Audiotica.Web.MatchEngine.Interfaces.Validators;
using Audiotica.Web.Models;

namespace Audiotica.Web.MatchEngine.Providers
{
    public class Mp3SkullMatchProvider : MatchProviderBase
    {
        public Mp3SkullMatchProvider(IEnumerable<ISongTypeValidator> validators, ISettingsUtility settingsUtility)
            : base(validators, settingsUtility)
        {
        }

        public override string DisplayName => "Mp3SKull";
        public override ProviderSpeed Speed => ProviderSpeed.Average;
        public override ProviderResultsQuality ResultsQuality => ProviderResultsQuality.BetterThanNothing;

        protected override Task<List<MatchSong>> InternalGetSongsAsync(string title, string artist, int limit = 10)
        {
            // TODO: finish mp3 clan provider
            throw new NotImplementedException();
        }
    }
}