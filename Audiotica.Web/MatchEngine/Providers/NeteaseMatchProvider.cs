using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Interfaces.Utilities;
using Audiotica.Web.Enums;
using Audiotica.Web.Http.Requets;
using Audiotica.Web.Http.Requets.MatchEngine.Netease;
using Audiotica.Web.MatchEngine.Interfaces;
using Audiotica.Web.MatchEngine.Interfaces.Validators;
using Audiotica.Web.Models;

namespace Audiotica.Web.MatchEngine.Providers
{
    public class NeteaseMatchProvider : MatchProviderBase
    {
        public NeteaseMatchProvider(IEnumerable<ISongTypeValidator> validators, ISettingsUtility settingsUtility)
            : base(validators, settingsUtility)
        {
        }

        public override string DisplayName => "Netease (163.com)";
        public override ProviderSpeed Speed => ProviderSpeed.Fast;
        public override ProviderResultsQuality ResultsQuality => ProviderResultsQuality.Excellent;

        protected async override Task<List<MatchSong>> InternalGetSongsAsync(string title, string artist, int limit = 10)
        {
            using (var response = await new NeteaseSearchRequest(title.Append(artist)).ToResponseAsync().ConfigureAwait(false))
            {
                var neteaseSongs = response.Data?.Result?.Songs?.Take(limit);
                if (neteaseSongs == null) return null;

                var songs = new List<MatchSong>();

                foreach (var neteaseSong in neteaseSongs)
                {
                    using (var detailsResponse = await new NeteaseDetailsRequest(neteaseSong.Id).ToResponseAsync().ConfigureAwait(false))
                    {
                        var song = detailsResponse.Data?.Songs?.FirstOrDefault();
                        if (song != null)
                            songs.Add(new MatchSong(song));
                    }
                }

                return songs;
            }
        }
    }
}