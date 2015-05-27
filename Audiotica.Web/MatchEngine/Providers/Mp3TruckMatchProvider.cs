using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Interfaces.Utilities;
using Audiotica.Web.Enums;
using Audiotica.Web.Http.Requets;
using Audiotica.Web.Http.Requets.MatchEngine.Mp3Truck;
using Audiotica.Web.MatchEngine.Interfaces;
using Audiotica.Web.MatchEngine.Interfaces.Validators;
using Audiotica.Web.Models;

namespace Audiotica.Web.MatchEngine.Providers
{
    public class Mp3TruckMatchProvider : MatchProviderBase
    {
        public Mp3TruckMatchProvider(IEnumerable<ISongTypeValidator> validators, ISettingsUtility settingsUtility)
            : base(validators, settingsUtility)
        {
        }

        public override string DisplayName => "Mp3Truck";
        public override ProviderSpeed Speed => ProviderSpeed.Average;
        public override ProviderResultsQuality ResultsQuality => ProviderResultsQuality.Great;

        protected override async Task<List<MatchSong>> InternalGetSongsAsync(string title, string artist, int limit = 10)
        {
            using (var response = await new Mp3TruckSearchRequest(title.Append(artist)).ToResponseAsync().DontMarshall()
                )
            {
                if (!response.HasData) return null;

                // Get the div node with the class='actl'
                var songNodes =
                    response.Data.DocumentNode.Descendants("div")
                        .Where(p => p.Attributes["class"]?.Value.Contains("actl") ?? false).Take(limit);

                var songs = new List<MatchSong>();

                foreach (var songNode in songNodes)
                {
                    var song = new MatchSong
                    {
                        Id = songNode.Attributes["data-id"]?.Value
                    };

                    int bitrate;
                    if (int.TryParse(songNode.Attributes["data-bitrate"]?.Value, out bitrate))
                    {
                        song.BitRate = bitrate;
                    }

                    var duration = songNode.Attributes["data-duration"]?.Value;
                    if (!string.IsNullOrEmpty(duration))
                    {
                        if (duration.Contains(":"))
                        {
                            var seconds = int.Parse(duration.Substring(duration.Length - 2, 2));
                            var minutes = int.Parse(duration.Remove(duration.Length - 3));
                            song.Duration = new TimeSpan(0, 0, minutes, seconds);
                        }
                        else
                        {
                            song.Duration = new TimeSpan(0, 0, 0, int.Parse(duration));
                        }
                    }

                    var songTitle = songNode.Descendants("div").FirstOrDefault(p => p.Id == "title")?.InnerText;
                    if (string.IsNullOrEmpty(songTitle)) continue;
                    song.Title = WebUtility.HtmlDecode(songTitle.Substring(0, songTitle.Length - 4)).Trim();

                    var linkNode = songNode.Descendants("a")
                        .FirstOrDefault(p => p.Attributes["class"]?.Value.Contains("mp3download") ?? false);
                    song.AudioUrl = linkNode?.Attributes["href"]?.Value.Replace("/idl.php?u=", string.Empty);

                    if (string.IsNullOrEmpty(song.AudioUrl)) continue;

                    songs.Add(song);
                }

                return songs;
            }
        }
    }
}