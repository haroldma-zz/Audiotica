using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Audiotica.Web.Interfaces.MatchEngine;
using Audiotica.Web.Interfaces.MatchEngine.Validators;
using Audiotica.Web.Models;

namespace Audiotica.Web.MatchEngine.Providers
{
    public class Mp3SkullProvider : ProviderBase
    {
        public Mp3SkullProvider(IEnumerable<ISongTypeValidator> validators) : base(validators)
        {
        }

        public override ProviderSpeed Speed => ProviderSpeed.Average;
        public override ProviderResultsQuality ResultsQuality => ProviderResultsQuality.BetterThanNothing;

        protected override async Task<List<WebSong>> InternalGetSongsAsync(string title, string artist, int limit = 10)
        {
            throw new NotImplementedException();
        }
    }
}