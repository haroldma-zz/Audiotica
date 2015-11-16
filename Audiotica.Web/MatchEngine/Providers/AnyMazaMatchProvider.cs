using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Web.Enums;
using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.MatchEngine.AnyMaza;
using Audiotica.Web.MatchEngine.Interfaces.Validators;
using Audiotica.Web.Models;
using HtmlAgilityPack;

namespace Audiotica.Web.MatchEngine.Providers
{
    public class AnyMazaMatchProvider : MatchProviderBase
    {
        public AnyMazaMatchProvider(IEnumerable<ISongTypeValidator> validators, ISettingsUtility settingsUtility)
            : base(validators, settingsUtility)
        {
        }

        public override string DisplayName => "AnyMaza.Com";

        public override ProviderSpeed Speed => ProviderSpeed.Fast;
        public override ProviderResultsQuality ResultsQuality => ProviderResultsQuality.Excellent;

        protected override async Task<List<MatchSong>> InternalGetSongsAsync(string title, string artist, int limit = 10)
        {
            using (var resp = await new AnyMazaSearchRequest(title.Append(artist)).ToResponseAsync())
            {
                if (!resp.HasData) return null;

                var songs = new List<MatchSong>();

                foreach (var googleResult in resp.Data.ResponseData.Results)
                {
                    var song = new MatchSong();

                    if (!googleResult.Url.StartsWith("http://anymaza.com/music/")) continue;

                    song.SetNameAndArtistFromTitle(googleResult.TitleNoFormatting.Remove(googleResult.TitleNoFormatting.LastIndexOf(artist, StringComparison.Ordinal) + artist.Length).Replace(" By ", " - "), false);

                    using (var anymazaResp = await googleResult.Url.ToUri().GetAsync())
                    {
                        if (!anymazaResp.IsSuccessStatusCode) continue;

                        var document = await anymazaResp.ParseHtmlAsync();

                        var duration = document.DocumentNode.Descendants("center").FirstOrDefault()?.InnerText;
                        //Category: Justin Bieber - Purpose ( Deluxe Edition ) Duration: 3:28 sec Singer : Justin Bieber
                        if (!string.IsNullOrEmpty(duration))
                        {
                            duration = duration.Substring(duration.IndexOf("Duration:", StringComparison.Ordinal) + 9);
                            duration = duration.Remove(duration.LastIndexOf("sec", StringComparison.Ordinal)).Trim();
                            TimeSpan dur;
                            if (TimeSpan.TryParse("00:" + duration, out dur))
                            {
                                song.Duration = dur;
                            }
                        }

                        var link = document.DocumentNode.Descendants("a")
                            .FirstOrDefault(p => p.Attributes["class"]?.Value == "dowbutzip")?.Attributes["href"]?.Value;

                        if (string.IsNullOrEmpty(link)) continue;

                        if (link.StartsWith("/"))
                            song.AudioUrl = "http://anymaza.com" + link;
                        else
                            song.AudioUrl = link;
                    }

                    songs.Add(song);
                }

                return songs;
            }
        }
    }
}