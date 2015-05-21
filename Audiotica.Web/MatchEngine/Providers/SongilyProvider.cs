using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Web.Http.Requets;
using Audiotica.Web.Interfaces.MatchEngine;
using Audiotica.Web.Interfaces.MatchEngine.Validators;
using Audiotica.Web.Models;

namespace Audiotica.Web.MatchEngine.Providers
{
    public class SongilyProvider : ProviderBase
    {
        public SongilyProvider(IEnumerable<ISongTypeValidator> validators) : base(validators)
        {
        }

        public override ProviderSpeed Speed => ProviderSpeed.Fast;
        public override ProviderResultsQuality ResultsQuality => ProviderResultsQuality.SomewhatGreat;

        protected override async Task<List<WebSong>> InternalGetSongsAsync(string title, string artist, int limit = 10)
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

                var songs = new List<WebSong>();

                foreach (var songNode in songNodes)
                {
                    var song = new WebSong();

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

                    song.Name = WebUtility.HtmlDecode(songTitle.InnerText).Trim();

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