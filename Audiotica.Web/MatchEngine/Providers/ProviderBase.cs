using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Web.Extensions;
using Audiotica.Web.Interfaces.MatchEngine;
using Audiotica.Web.Interfaces.MatchEngine.Validators;
using Audiotica.Web.Models;

namespace Audiotica.Web.MatchEngine.Providers
{
    /// <summary>
    ///     A base that simplifies implementing an <see cref="IProvider" />.
    /// </summary>
    public abstract class ProviderBase : IProvider
    {
        private readonly IEnumerable<ISongTypeValidator> _validators;

        protected ProviderBase(IEnumerable<ISongTypeValidator> validators)
        {
            _validators = validators;
        }

        public abstract ProviderSpeed Speed { get; }
        public abstract ProviderResultsQuality ResultsQuality { get; }

        public async Task<string> GetLinkAsync(string title, string artist)
        {
            var songs = await GetSongsAsync(title, artist).DontMarshall();
            return songs?.FirstOrDefault(p => p.IsBestMatch)?.AudioUrl;
        }

        public async Task<List<WebSong>> GetSongsAsync(string title, string artist,
            int limit = 10)
        {
            return
                await
                    VerifyMatchesAsync(title, artist, await InternalGetSongsAsync(title, artist, limit).DontMarshall())
                        .DontMarshall();
        }

        protected abstract Task<List<WebSong>> InternalGetSongsAsync(string title, string artist,
            int limit = 10);

        private const int MatchTitleLenghtThreshold = 20;

        private async Task<List<WebSong>> VerifyMatchesAsync(string title, string artist,
            IEnumerable<WebSong> matches, bool verifyMatchesOnly = true)
        {
            var webSongs = matches?.OrderByDescending(p => p.Duration.Minutes).ToList();
            if (webSongs == null) throw new ArgumentNullException(nameof(webSongs));

            title = title.ToAudioticaSlug();
            artist = artist.ToAudioticaSlug();

            // ReSharper disable once LoopCanBePartlyConvertedToQuery (code in this case looks more readable as it is)
            foreach (var webSong in webSongs)
            {
                var matchName = webSong.Name.ToAudioticaSlug();
                var matchArtist = webSong.Artist.ToAudioticaSlug();

                if (string.IsNullOrEmpty(matchName)) continue;

                // Run all type validators *VERY IMPORTANT*
                if (!_validators.All(songTypeValidator => songTypeValidator.IsValid(title, matchName))) continue;

                var titleDiff = Math.Abs(title.Length - matchName.Length);
                var isCorrectTitle = (matchName.Contains(title) || title.Contains(matchName)) 
                    && titleDiff <= MatchTitleLenghtThreshold + (string.IsNullOrEmpty(matchArtist) ? artist.Length : 0);
                if (!isCorrectTitle) continue;

                // soundcloud/youtube doesnt have artist prop, check in title and author name (channel/username) for those cases
                var isCorrectArtist = matchArtist != null
                    ? matchArtist.Contains(artist) || artist.Contains(matchArtist)
                    : matchName.Contains(artist)
                      || (webSong.FileAuthor != null && webSong.FileAuthor.ToLower().Contains(artist.Replace(" ", "")));
                if (!isCorrectArtist) continue;

                webSong.IsMatch = true;
            }

            var filterSongs = webSongs.Where(p => p.IsMatch).ToList();

            foreach (var webSong in
                (verifyMatchesOnly ? filterSongs : webSongs)
                    .Where(webSong => !string.IsNullOrEmpty(webSong.AudioUrl)
                                      && webSong.AudioUrl.StartsWith("http")))
            {
                if (await CheckAudioSizeAsync(webSong).DontMarshall())
                    webSong.IsBestMatch = webSong.IsMatch;
                else
                    webSong.IsLinkDeath = true;
            }

            var mostUsedMinute = filterSongs.Where(p => !p.IsLinkDeath).GetMostUsedOccurrenceWhileIgnoringZero(p => p.Duration.Minutes);
            webSongs.Where(p => p.IsBestMatch).ForEach(p => p.IsBestMatch = p.Duration.Minutes == mostUsedMinute);

            return webSongs;
        }

        private async Task<bool> CheckAudioSizeAsync(WebSong webSong)
        {
            using (var response = await webSong.AudioUrl.ToUri().HeadAsync())
            {
                if (response == null || !response.IsSuccessStatusCode) return false;

                var type = response.Content.Headers.ContentType?.MediaType ?? response.Content.Headers.GetValues("Content-Type")?.FirstOrDefault() ?? "";
                if (!type.Contains("audio") && !type.Contains("octet-stream"))
                    return false;

                var size = response.Content.Headers.ContentLength;
                if (size != null)
                {
                    webSong.ByteSize = (long) size;
                }
                return webSong.ByteSize > 0;
            }
        }
    }
}