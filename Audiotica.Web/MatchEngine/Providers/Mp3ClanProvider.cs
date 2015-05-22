using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Interfaces.Utilities;
using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets;
using Audiotica.Web.Interfaces.MatchEngine;
using Audiotica.Web.Interfaces.MatchEngine.Validators;
using Audiotica.Web.Models;

namespace Audiotica.Web.MatchEngine.Providers
{
    public class Mp3ClanProvider : ProviderBase
    {
        public Mp3ClanProvider(IEnumerable<ISongTypeValidator> validators, ISettingsUtility settingsUtility)
            : base(validators, settingsUtility)
        {
        }

        public override string DisplayName => "Mp3Clan";
        public override ProviderSpeed Speed => ProviderSpeed.Slow;
        public override ProviderResultsQuality ResultsQuality => ProviderResultsQuality.Great;

        protected override async Task<List<WebSong>> InternalGetSongsAsync(string title, string artist, int limit = 10)
        {
            using (var response = await new Mp3ClanSearchRequest(title.Append(artist)).ToResponseAsync().DontMarshall())
            {
                if (!response.HasData) return null;

                var songs = new List<WebSong>();

                var songNodes =
                    response.Data.DocumentNode.Descendants("div").Where(p => p.Id == "mp3list-tr").Take(limit);

                foreach (var songNode in songNodes)
                {
                    var song = new WebSong();

                    var songTitle = songNode.Descendants("div")
                        .FirstOrDefault(p => p.Attributes["class"]?.Value == "unselectable")?.InnerText;

                    var link = songNode.Descendants("a")
                        .FirstOrDefault(p => p.Attributes.Contains("download"))?.Attributes["href"]?.Value;

                    if (string.IsNullOrEmpty(link)) continue;
                    song.AudioUrl = link;

                    if (string.IsNullOrEmpty(songTitle)) continue;
                    song.SetNameAndArtistFromTitle(songTitle, true);

                    var duration = songNode.Descendants("div")
                        .FirstOrDefault(p => p.Attributes["class"]?.Value.Contains("mp3list-bitrate") ?? false)?
                        .InnerHtml;
                    if (!string.IsNullOrEmpty(duration))
                    {
                        duration = "0:" + duration.Substring(duration.IndexOf("<br>") + 4).Replace(" min", "").Trim();
                        TimeSpan timespan;
                        if (TimeSpan.TryParse(duration, out timespan))
                            song.Duration = timespan;
                    }

                    songs.Add(song);
                }

                return songs;
            }
        }
    }
}