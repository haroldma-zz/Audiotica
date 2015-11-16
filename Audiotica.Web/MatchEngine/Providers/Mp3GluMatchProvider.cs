using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Web.Enums;
using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.MatchEngine.Mp3Truck;
using Audiotica.Web.MatchEngine.Interfaces.Validators;
using Audiotica.Web.Models;

namespace Audiotica.Web.MatchEngine.Providers
{
    public class Mp3GluMatchProvider : MatchProviderBase
    {
        public Mp3GluMatchProvider(IEnumerable<ISongTypeValidator> validators, ISettingsUtility settingsUtility)
            : base(validators, settingsUtility)
        {
        }

        public override string DisplayName => "Mp3Glu";
        public override ProviderSpeed Speed => ProviderSpeed.Fast;
        public override ProviderResultsQuality ResultsQuality => ProviderResultsQuality.Great;

        protected override async Task<List<MatchSong>> InternalGetSongsAsync(string title, string artist, int limit = 10)
        {
            using (var response = await new Mp3GluSearchRequest(title.Append(artist)).ToResponseAsync().DontMarshall()
                )
            {
                if (!response.HasData) return null;
                
                var songNodes =
                    response.Data.DocumentNode.Descendants("div")
                        .Where(p => p.Attributes["class"]?.Value.Contains("result_list") ?? false).Take(limit);

                var songs = new List<MatchSong>();

                foreach (var songNode in songNodes)
                {
                    var song = new MatchSong();

                    var detailsNode = songNode.Descendants("em").FirstOrDefault();
                    
                    var duration = detailsNode.InnerText.Substring(0, detailsNode.InnerText.IndexOf("min ", StringComparison.Ordinal));
                    if (!string.IsNullOrEmpty(duration))
                    {
                        duration = duration.Replace("Duration : ", "").Trim();
                        var seconds = int.Parse(duration.Substring(duration.Length - 2, 2));
                        var minutes = int.Parse(duration.Remove(duration.Length - 3));
                        song.Duration = new TimeSpan(0, 0, minutes, seconds);
                    }

                    var songTitle = songNode.Descendants("strong").FirstOrDefault()?.InnerText;
                    if (string.IsNullOrEmpty(songTitle)) continue;
                    song.SetNameAndArtistFromTitle(WebUtility.HtmlDecode(songTitle.Substring(0, songTitle.Length - 4)).Trim(), true);

                    var linkNode = songNode.Descendants("a")
                        .FirstOrDefault();
                    song.AudioUrl = linkNode?.Attributes["href"]?.Value;

                    if (string.IsNullOrEmpty(song.AudioUrl)) continue;

                    songs.Add(song);
                }

                return songs;
            }
        }
    }
}