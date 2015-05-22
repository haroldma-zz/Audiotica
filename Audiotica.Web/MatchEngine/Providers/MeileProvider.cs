using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Interfaces.Utilities;
using Audiotica.Web.Http.Requets;
using Audiotica.Web.Interfaces.MatchEngine;
using Audiotica.Web.Interfaces.MatchEngine.Validators;
using Audiotica.Web.Models;
using Audiotica.Web.Models.Meile;

namespace Audiotica.Web.MatchEngine.Providers
{
    public class MeileProvider : ProviderBase
    {
        public MeileProvider(IEnumerable<ISongTypeValidator> validators, ISettingsUtility settingsUtility)
            : base(validators, settingsUtility)
        {
        }

        public override string DisplayName => "Meile";
        public override ProviderSpeed Speed => ProviderSpeed.Slow;
        public override ProviderResultsQuality ResultsQuality => ProviderResultsQuality.Great;

        protected override async Task<List<WebSong>> InternalGetSongsAsync(string title, string artist, int limit = 10)
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

                var songs = new List<WebSong>();

                foreach (var songId in songIds)
                {
                    var song = await GetDetailsAsync(songId.Replace("/song/", string.Empty)).DontMarshall();
                    if (song != null)
                    {
                        songs.Add(new WebSong(song));
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