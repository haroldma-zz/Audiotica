using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Interfaces.Utilities;
using Audiotica.Web.Http.Requets;
using Audiotica.Web.Interfaces.MatchEngine;
using Audiotica.Web.Interfaces.MatchEngine.Validators;
using Audiotica.Web.Models;

namespace Audiotica.Web.MatchEngine.Providers
{
    public class NeteaseProvider : ProviderBase
    {
        public NeteaseProvider(IEnumerable<ISongTypeValidator> validators, ISettingsUtility settingsUtility)
            : base(validators, settingsUtility)
        {
        }

        public override string DisplayName => "Netease (163.com)";
        public override ProviderSpeed Speed => ProviderSpeed.Fast;
        public override ProviderResultsQuality ResultsQuality => ProviderResultsQuality.Excellent;

        protected async override Task<List<WebSong>> InternalGetSongsAsync(string title, string artist, int limit = 10)
        {
            using (var response = await new NeteaseSearchRequest(title.Append(artist)).ToResponseAsync().ConfigureAwait(false))
            {
                var neteaseSongs = response.Data?.Result?.Songs?.Take(limit);
                if (neteaseSongs == null) return null;

                var songs = new List<WebSong>();

                foreach (var neteaseSong in neteaseSongs)
                {
                    using (var detailsResponse = await new NeteaseDetailsRequest(neteaseSong.Id).ToResponseAsync().ConfigureAwait(false))
                    {
                        var song = detailsResponse.Data?.Songs?.FirstOrDefault();
                        if (song != null)
                            songs.Add(new WebSong(song));
                    }
                }

                return songs;
            }
        }
    }
}