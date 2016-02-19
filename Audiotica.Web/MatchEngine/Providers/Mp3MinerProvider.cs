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
using System.Text.RegularExpressions;

namespace Audiotica.Web.MatchEngine.Providers
{
    public class Mp3MinerProvider : MatchProviderBase
    {
        public Mp3MinerProvider(IEnumerable<ISongTypeValidator> validators, ISettingsUtility settingsUtility)
            : base(validators, settingsUtility)
        {
        }

        public override ProviderSpeed Speed => ProviderSpeed.Fast;
        public override ProviderResultsQuality ResultsQuality => ProviderResultsQuality.Excellent;
        public override string DisplayName => "Mp3Miner";

        protected override async Task<List<MatchSong>> InternalGetSongsAsync(string title, string artist, int limit = 10)
        {
            using (var response = await new Mp3MinerSearchRequest("mp3songs", title.Append(artist)).ToResponseAsync())
            {
                if (!response.HasData) return null;

                var songNodes =
                    response.Data.DocumentNode.Descendants("a")
                        .Where(p => p.Attributes.Contains("href") && p.Attributes["href"].Value.Contains("ref=search"))
                        .Take(limit);

                var songs = new List<MatchSong>();

                foreach (var songNode in songNodes)
                {
                    using (var InnerResponse = await new Mp3MinerSearchRequest("mp3", songNode.Attributes["href"].Value.Substring(5)).ToResponseAsync())
                    {
                        if (!InnerResponse.HasData)
                        {
                            return null;
                        }

                        var songUrl =
                            InnerResponse.Data.DocumentNode.Descendants("audio")
                                .Where(p => p.Attributes.Contains("src"))
                                .FirstOrDefault();
                        
                        if(songUrl == null)
                        {
                            break;
                        }

                        var innerSongNode = 
                            InnerResponse.Data.DocumentNode.Descendants("div")
                                .Where(p => p.Attributes.Contains("class") && p.Attributes["class"].Value.Contains("mdl-card__title"))
                                .FirstOrDefault();

                        if(innerSongNode == null)
                        {
                            break;
                        }

                        var song = new MatchSong
                        {
                            Id = Regex.Match(songNode.Attributes["href"].Value, "/mp3/(.*?)-").Groups[1].ToString(),
                            AudioUrl = songUrl.Attributes["src"]?.Value
                        };

                        if (string.IsNullOrEmpty(song.AudioUrl)) continue;

                        var titleText =
                            innerSongNode.Descendants("h2")
                                .FirstOrDefault(p => p.Attributes["class"]?.Value == "mdl-card__title-text")?.InnerText;
                        if (string.IsNullOrEmpty(titleText)) continue;
                        song.Title = WebUtility.HtmlDecode(titleText);

                        var artistText =
                            innerSongNode.Descendants("h3")
                                .FirstOrDefault().ChildNodes.FirstOrDefault(c => c.Attributes.Contains("href"))?.InnerText;
                        if (string.IsNullOrEmpty(artistText)) continue;
                        song.Artist = WebUtility.HtmlDecode(artistText);

                        var durationText =
                            InnerResponse.Data.DocumentNode.Descendants("div")
                                .FirstOrDefault(p => p.Attributes["class"]?.Value == "info-section stretchable")?.InnerText;
                        durationText = durationText.Substring(durationText.IndexOf("/") + 1).Trim();
                        TimeSpan duration;
                        if (TimeSpan.TryParse("00:" + durationText, out duration))
                        {
                            song.Duration = duration;
                        }

                        songs.Add(song);
                    }
                }

                return songs;
            }
        }
    }
}