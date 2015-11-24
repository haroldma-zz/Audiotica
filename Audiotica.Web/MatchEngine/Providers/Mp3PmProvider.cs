using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Web.Enums;
using Audiotica.Web.Http.Requets.MatchEngine.Mp3Pm;
using Audiotica.Web.MatchEngine.Interfaces.Validators;
using Audiotica.Web.Models;

namespace Audiotica.Web.MatchEngine.Providers
{
    public class Mp3PmProvider : MatchProviderBase
    {
        public Mp3PmProvider(IEnumerable<ISongTypeValidator> validators, ISettingsUtility settingsUtility)
            : base(validators, settingsUtility)
        {
        }

        public override ProviderSpeed Speed => ProviderSpeed.Fast;
        public override ProviderResultsQuality ResultsQuality => ProviderResultsQuality.Excellent;
        public override string DisplayName => "Mp3Pm";

        protected override async Task<List<MatchSong>> InternalGetSongsAsync(string title, string artist, int limit = 10)
        {
            // in the query, the artist goes first. Results don't work otherwise.
            using (var response = await new Mp3PmSearchRequest(artist.Append(title)).ToResponseAsync())
            {
                if (!response.HasData) return null;

                var songNodes =
                    response.Data.DocumentNode.Descendants("li")
                        .Where(p => p.Attributes.Contains("data-sound-url"))
                        .Take(limit);

                var songs = new List<MatchSong>();

                foreach (var songNode in songNodes)
                {
                    var song = new MatchSong
                    {
                        Id = songNode.Attributes["data-sound-id"]?.Value,
                        AudioUrl = songNode.Attributes["data-sound-url"]?.Value
                    };

                    if (string.IsNullOrEmpty(song.AudioUrl)) continue;

                    var titleText =
                        songNode.Descendants("b")
                            .FirstOrDefault(p => p.Attributes["class"]?.Value == "cplayer-data-sound-title")?.InnerText;
                    if (string.IsNullOrEmpty(titleText)) continue;
                    song.Title = WebUtility.HtmlDecode(titleText);

                    var artistText =
                        songNode.Descendants("i")
                            .FirstOrDefault(p => p.Attributes["class"]?.Value == "cplayer-data-sound-author")?.InnerText;
                    if (string.IsNullOrEmpty(artistText)) continue;
                    song.Artist = WebUtility.HtmlDecode(artistText);

                    var durationText =
                        songNode.Descendants("em")
                            .FirstOrDefault(p => p.Attributes["class"]?.Value == "cplayer-data-sound-time")?.InnerText;
                    TimeSpan duration;
                    if (TimeSpan.TryParse("00:" + durationText, out duration))
                    {
                        song.Duration = duration;
                    }

                    songs.Add(song);
                }

                return songs;
            }
        }
    }
}