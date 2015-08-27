using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Web.Enums;
using Audiotica.Web.Http.Requets.MatchEngine.Pleer;
using Audiotica.Web.MatchEngine.Interfaces.Validators;
using Audiotica.Web.Models;
using HtmlAgilityPack;

namespace Audiotica.Web.MatchEngine.Providers
{
    public class PleerMatchProvider : MatchProviderBase
    {
        public PleerMatchProvider(IEnumerable<ISongTypeValidator> validators, ISettingsUtility settingsUtility)
            : base(validators, settingsUtility)
        {
        }

        public override string DisplayName => "ProstoPleer";
        public override ProviderSpeed Speed => ProviderSpeed.SuperSlow;
        public override ProviderResultsQuality ResultsQuality => ProviderResultsQuality.SomewhatGreat;

        protected override async Task<List<MatchSong>> InternalGetSongsAsync(string title, string artist, int limit = 10)
        {
            using (var response = await new PleerSearchRequest(title.Append(artist)).ToResponseAsync().DontMarshall())
            {
                var o = response.Data;
                if (!response.HasData || !o.Value<bool>("success")) return null;

                var html = o.Value<string>("html");

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var songNodes =
                    doc.DocumentNode.Descendants("li").Where(p => p.Attributes.Contains("file_id")).Take(limit);

                var songs = new List<MatchSong>();

                foreach (var songNode in songNodes)
                {
                    var song = new MatchSong
                    {
                        Id = songNode.Attributes["file_id"].Value,
                        Title = songNode.Attributes["song"].Value,
                        Artist = songNode.Attributes["singer"].Value
                    };


                    int bitRate;
                    if (int.TryParse(songNode.Attributes["rate"].Value.Replace(" Kb/s", ""), out bitRate))
                    {
                        song.BitRate = bitRate;
                    }
                    int seconds;
                    if (int.TryParse(songNode.Attributes["duration"].Value, out seconds))
                    {
                        song.Duration = TimeSpan.FromSeconds(seconds);
                    }

                    var linkId = songNode.Attributes["link"].Value;
                    song.AudioUrl = await GetPleerLinkAsync(linkId);

                    if (string.IsNullOrEmpty(song.AudioUrl)) continue;

                    songs.Add(song);
                }

                return songs;
            }
        }

        private async Task<string> GetPleerLinkAsync(string id)
        {
            using (var response = await new PleerDetailsRequest(id).ToResponseAsync().DontMarshall())
            {
                return response.Data?.Value<string>("track_link");
            }
        }
    }
}