using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Web.Enums;
using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.MatchEngine.Mp3lio;
using Audiotica.Web.MatchEngine.Interfaces.Validators;
using Audiotica.Web.Models;

namespace Audiotica.Web.MatchEngine.Providers
{
    public class Mp3lioMatchProvider : MatchProviderBase
    {
        public Mp3lioMatchProvider(IEnumerable<ISongTypeValidator> validators, ISettingsUtility settingsUtility)
            : base(validators, settingsUtility)
        {
        }

        public override string DisplayName => "Mp3lio";
        public override ProviderSpeed Speed => ProviderSpeed.Fast;
        public override ProviderResultsQuality ResultsQuality => ProviderResultsQuality.NotSoGreat;

        protected override async Task<List<MatchSong>> InternalGetSongsAsync(string title, string artist, int limit = 10)
        {
            using (var response = await new Mp3lioSearchRequest(title.Append(artist)).ToResponseAsync().DontMarshall()
                )
            {
                if (!response.HasData) return null;
                
                var songNodes =
                    response.Data.DocumentNode.Descendants("div")
                        .Where(p => p.Attributes["class"]?.Value.Contains("item") ?? false).Take(limit);

                var songs = new List<MatchSong>();

                foreach (var songNode in songNodes)
                {
                    var song = new MatchSong();
                    
                    var duration = songNode.Descendants("em").FirstOrDefault();
                    if (duration != null)
                    {
                        var text = duration.InnerText.Replace("Duration: ", "").Replace(" min", "");
                        var seconds = int.Parse(text.Substring(text.Length - 2, 2));
                        var minutes = int.Parse(text.Remove(text.Length - 3));
                        song.Duration = new TimeSpan(0, 0, minutes, seconds);
                    }

                    var songTitle = songNode.Descendants("strong").FirstOrDefault()?.InnerText;
                    if (string.IsNullOrEmpty(songTitle)) continue;
                    song.Title = songTitle.Substring(0, songTitle.Length - 4);

                    var linkNode = songNode.Descendants("a").FirstOrDefault();
                    song.AudioUrl = linkNode?.Attributes["href"]?.Value;

                    if (string.IsNullOrEmpty(song.AudioUrl)) continue;

                    songs.Add(song);
                }

                return songs;
            }
        }
    }
}