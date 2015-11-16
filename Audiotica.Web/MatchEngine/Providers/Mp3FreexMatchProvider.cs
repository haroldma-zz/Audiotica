using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Web.Enums;
using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.MatchEngine.Mp3Freex;
using Audiotica.Web.MatchEngine.Interfaces.Validators;
using Audiotica.Web.Models;

namespace Audiotica.Web.MatchEngine.Providers
{
    public class Mp3FreexMatchProvider : MatchProviderBase
    {
        public Mp3FreexMatchProvider(IEnumerable<ISongTypeValidator> validators, ISettingsUtility settingsUtility)
            : base(validators, settingsUtility)
        {
        }

        public override string DisplayName => "Mp3Freex";
        public override ProviderSpeed Speed => ProviderSpeed.Average;
        public override ProviderResultsQuality ResultsQuality => ProviderResultsQuality.Great;

        protected override async Task<List<MatchSong>> InternalGetSongsAsync(string title, string artist, int limit = 10)
        {
            using (var response = await new Mp3FreexSearchRequest(title.Append(artist)).ToResponseAsync().DontMarshall()
                )
            {
                if (!response.HasData) return null;

                var songNodes =
                    response.Data.DocumentNode.Descendants("div")
                        .Where(p => p.Attributes["class"]?.Value.Contains("actl") ?? false).Take(limit);

                var songs = new List<MatchSong>();

                foreach (var songNode in songNodes)
                {
                    var song = new MatchSong();

                    var songTitle =
                        songNode.Descendants("span")
                            .FirstOrDefault(p => p.Attributes["class"]?.Value == "res_title")?
                            .InnerText;
                    if (string.IsNullOrEmpty(songTitle)) continue;
                    song.SetNameAndArtistFromTitle(songTitle, true);

                    var meta =
                        songNode.Descendants("span")
                            .FirstOrDefault(p => p.Attributes["class"]?.Value == "label label-info")?
                            .InnerText;

                    if (!string.IsNullOrEmpty(meta))
                    {
                        var duration = meta.Substring(0, meta.IndexOf("|", StringComparison.Ordinal)).Trim();
                        var seconds = int.Parse(duration.Substring(duration.Length - 2, 2));
                        var minutes = int.Parse(duration.Remove(duration.Length - 3));
                        song.Duration = new TimeSpan(0, 0, minutes, seconds);

                        int bitrate;
                        if (int.TryParse(meta.Substring(meta.IndexOf("|", StringComparison.Ordinal) + 1).Replace("kbps", ""), out bitrate))
                        {
                            song.BitRate = bitrate;
                        }
                    }

                    var linkNode =
                        songNode.Descendants("a")
                            .FirstOrDefault(p => p.Attributes["class"]?.Value.Contains("mp3download") ?? false);
                    var url = linkNode?.Attributes["data-online"]?.Value;

                    if (string.IsNullOrEmpty(url)) continue;

                    song.AudioUrl = await GetAudioUrlAsync(url).DontMarshall();

                    if (string.IsNullOrEmpty(song.AudioUrl)) continue;

                    songs.Add(song);
                }

                return songs;
            }
        }

        private async Task<string> GetAudioUrlAsync(string downloadPart)
        {
            using (var response = await ("http://mp3freex.net/listen-online/" + downloadPart).ToUri().GetAsync().DontMarshall())
            {
                if (!response.IsSuccessStatusCode) return null;

                var doc = await response.ParseHtmlAsync().DontMarshall();

                var linkNode = doc.DocumentNode.Descendants("div").FirstOrDefault(p => p.Attributes["class"]?.Value.Contains("player") ?? false);
                return linkNode?.Attributes["data-url"]?.Value;
            }
        }
    }
}