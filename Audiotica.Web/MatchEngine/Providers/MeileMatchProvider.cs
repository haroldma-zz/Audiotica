using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Web.Enums;
using Audiotica.Web.Http.Requets.MatchEngine.Meile;
using Audiotica.Web.Http.Requets.MatchEngine.Meile.Models;
using Audiotica.Web.MatchEngine.Interfaces.Validators;
using Audiotica.Web.Models;

namespace Audiotica.Web.MatchEngine.Providers
{
    public class MeileMatchProvider : MatchProviderBase
    {
        public MeileMatchProvider(IEnumerable<ISongTypeValidator> validators, ISettingsUtility settingsUtility)
            : base(validators, settingsUtility)
        {
        }

        public override string DisplayName => "Meile";
        public override ProviderSpeed Speed => ProviderSpeed.SuperSlow;
        public override ProviderResultsQuality ResultsQuality => ProviderResultsQuality.Great;

        protected override async Task<List<MatchSong>> InternalGetSongsAsync(string title, string artist, int limit = 10)
        {
            using (var response = await new MeileSearchRequest(title.Append(artist))
                .Limit(limit).ToResponseAsync().DontMarshall())
            {
                if (!response.HasData) return null;
                var htmlDocument = response.Data;

                // Get the hyperlink node with the class='name'
                var songNameNodes =
                    htmlDocument.DocumentNode.Descendants("a")
                        .Where(p => p.Attributes.Contains("class") && p.Attributes["class"].Value == "name");

                // in it there is an attribute that contains the url to the song
                var songUrls = songNameNodes.Select(p => p.Attributes["href"].Value);
                var songIds = songUrls.Where(p => p.Contains("/song/")).ToList();

                var songs = new List<MatchSong>();

                foreach (var songId in songIds)
                {
                    var song = await GetDetailsAsync(songId.Replace("/song/", string.Empty)).DontMarshall();
                    if (song != null)
                    {
                        songs.Add(new MatchSong(song));
                    }
                }

                return songs;
            }
        }

        protected async Task<MeileSong> GetDetailsAsync(string id)
        {
            using (var response = await new MeileDetailsRequest(id).ToResponseAsync().DontMarshall())
            {
                return response.Data?.Values?.Songs?.FirstOrDefault();
            }
        }
    }
}