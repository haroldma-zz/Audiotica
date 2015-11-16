using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Web.Enums;
using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.MatchEngine.MiMp3S;
using Audiotica.Web.MatchEngine.Interfaces.Validators;
using Audiotica.Web.Models;

namespace Audiotica.Web.MatchEngine.Providers
{
    public class MiMp3SMatchProvider : MatchProviderBase
    {
        public MiMp3SMatchProvider(IEnumerable<ISongTypeValidator> validators, ISettingsUtility settingsUtility)
            : base(validators, settingsUtility)
        {
        }

        public override string DisplayName => "MiMP3's";

        public override ProviderSpeed Speed => ProviderSpeed.Fast;
        public override ProviderResultsQuality ResultsQuality => ProviderResultsQuality.BetterThanNothing;

        protected override async Task<List<MatchSong>> InternalGetSongsAsync(string title, string artist, int limit = 10)
        {
            using (var resp = await new MiMp3SSearchRequest(title.Append(artist).ToLower()).ToResponseAsync())
            {
                if (!resp.HasData) return null;

                var songNodes =
                    resp.Data.DocumentNode.Descendants("ul")
                        .FirstOrDefault(p => p.Attributes["class"]?.Value == "mp3-list").Descendants("li");

                var songs = new List<MatchSong>();

                foreach (var songNode in songNodes)
                {
                    var song = new MatchSong();

                    var titleNode =
                        songNode.Descendants("span").FirstOrDefault(p => p.Attributes["class"]?.Value == "mp3-title");
                    if (titleNode == null) continue;

                    titleNode.Descendants("font").FirstOrDefault().Remove();
                    var songTitle = titleNode.InnerText;
                    if (string.IsNullOrEmpty(songTitle)) continue;

                    song.Title = songTitle.Remove(songTitle.LastIndexOf(" - MP3", StringComparison.Ordinal)).Trim();

                    var meta =
                        WebUtility.HtmlDecode(
                            songNode.Descendants("span")
                                .FirstOrDefault(p => p.Attributes["class"]?.Value == "mp3-url")
                                .InnerText);
                    if (!string.IsNullOrEmpty(meta))
                    {
                        var duration = meta.Substring(10, meta.IndexOf("•", StringComparison.Ordinal) - 10).Trim();
                        TimeSpan parsed;
                        if (TimeSpan.TryParse("00:" + duration, out parsed))
                        {
                            song.Duration = parsed;
                        }
                    }

                    var linkNode =
                        songNode.Descendants("a").FirstOrDefault(p => p.Attributes["class"]?.Value == "play_button");
                    song.AudioUrl = linkNode.Attributes["href"]?.Value;

                    if (string.IsNullOrEmpty(song.AudioUrl)) continue;
                    song.AudioUrl = await GetAudioUrlAsync(song.AudioUrl);
                    if (string.IsNullOrEmpty(song.AudioUrl)) continue;

                    songs.Add(song);
                }

                return songs.Take(limit).ToList();
            }
        }

        private async Task<string> GetAudioUrlAsync(string url)
        {
            using (var response = await (url).ToUri().GetAsync().DontMarshall())
            {
                if (!response.IsSuccessStatusCode) return null;

                var doc = await response.ParseHtmlAsync().DontMarshall();

                var linkNode = doc.DocumentNode.Descendants("div")
                    .FirstOrDefault(p => p.Attributes.Contains("data-url"));
                return linkNode?.Attributes["data-url"]?.Value;
            }
        }
    }
}