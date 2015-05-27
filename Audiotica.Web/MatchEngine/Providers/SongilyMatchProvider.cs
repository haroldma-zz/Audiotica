using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Interfaces.Utilities;
using Audiotica.Web.Enums;
using Audiotica.Web.Http.Requets;
using Audiotica.Web.Http.Requets.MatchEngine.Songily;
using Audiotica.Web.MatchEngine.Interfaces;
using Audiotica.Web.MatchEngine.Interfaces.Validators;
using Audiotica.Web.Models;

namespace Audiotica.Web.MatchEngine.Providers
{
    public class SongilyMatchProvider : MatchProviderBase
    {
        public SongilyMatchProvider(IEnumerable<ISongTypeValidator> validators, ISettingsUtility settingsUtility)
            : base(validators, settingsUtility)
        {
        }

        public override string DisplayName => "Songily";
        public override ProviderSpeed Speed => ProviderSpeed.Fast;
        public override ProviderResultsQuality ResultsQuality => ProviderResultsQuality.SomewhatGreat;

        protected override async Task<List<MatchSong>> InternalGetSongsAsync(string title, string artist, int limit = 10)
        {
            using (var response = await new SongilySearchRequest(title.Append(artist))
                .Page(1).ToResponseAsync().DontMarshall())
            {
                if (!response.HasData) return null;
                var doc = response.Data;

                // Get the div node with the class='actl'
                var songNodes =
                    doc.DocumentNode.Descendants("li")
                        .Where(
                            p =>
                                p.Attributes.Contains("class") &&
                                p.Attributes["class"].Value.Contains("list-group-item")).Take(10);

                var songs = new List<MatchSong>();

                foreach (var songNode in songNodes)
                {
                    var song = new MatchSong();

                    var detailNode = songNode.Descendants("small").FirstOrDefault();
                    if (detailNode != null)
                    {
                        var duration = detailNode.InnerText;

                        var durIndex = duration.IndexOf(":");
                        if (durIndex > 0)
                        {
                            var seconds = int.Parse(duration.Substring(durIndex + 1, 2));
                            var minutes = int.Parse(duration.Substring(0, durIndex));
                            ;
                            song.Duration = new TimeSpan(0, 0, minutes, seconds);
                        }
                    }

                    var songTitle =
                        songNode.Descendants("span")
                            .FirstOrDefault();

                    if (songTitle == null) continue;

                    song.Title = WebUtility.HtmlDecode(songTitle.InnerText).Trim();

                    var linkNode =
                        songNode.Descendants("a")
                            .FirstOrDefault(
                                p =>
                                    p.Attributes.Contains("title")
                                    && p.Attributes["title"].Value.Contains("Download"));
                    if (linkNode == null)
                    {
                        continue;
                    }

                    song.AudioUrl = "http://songily.com/" + linkNode.Attributes["href"].Value;

                    songs.Add(song);
                }

                return songs;
            }
        }
    }
}