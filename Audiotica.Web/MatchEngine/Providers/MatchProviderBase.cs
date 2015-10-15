using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Web.Enums;
using Audiotica.Web.Extensions;
using Audiotica.Web.MatchEngine.Interfaces;
using Audiotica.Web.MatchEngine.Interfaces.Validators;
using Audiotica.Web.Models;

namespace Audiotica.Web.MatchEngine.Providers
{
    /// <summary>
    ///     A base that simplifies implementing an <see cref="IMatchProvider" />.
    /// </summary>
    public abstract class MatchProviderBase : IMatchProvider
    {
        private const int MatchTitleLenghtThreshold = 20;
        private readonly ISettingsUtility _settingsUtility;
        private readonly IEnumerable<ISongTypeValidator> _validators;

        protected MatchProviderBase(IEnumerable<ISongTypeValidator> validators, ISettingsUtility settingsUtility)
        {
            _validators = validators;
            _settingsUtility = settingsUtility;
        }

        public abstract ProviderSpeed Speed { get; }
        public abstract ProviderResultsQuality ResultsQuality { get; }
        public abstract string DisplayName { get; }

        public bool IsEnabled
        {
            get { return _settingsUtility.Read($"match_provider_enabled_{DisplayName}", true); }

            set { _settingsUtility.Write($"match_provider_enabled_{DisplayName}", value); }
        }

        public int Priority => (int) ResultsQuality + (int) Speed;

        public async Task<Uri> GetLinkAsync(string title, string artist)
        {
            var songs = await GetSongsAsync(title, artist).DontMarshall();
            var uriString = songs?.FirstOrDefault(p => p.IsBestMatch)?.AudioUrl;

            return Uri.IsWellFormedUriString(uriString, UriKind.Absolute) ? new Uri(uriString, UriKind.Absolute) : null;
        }
        
        public async Task<List<MatchSong>> GetSongsAsync(string title, string artist,
            int limit = 10, bool verifyMatchesOnly = true)
        {
            return
                await
                    VerifyMatchesAsync(title, artist, await InternalGetSongsAsync(title, artist, limit).DontMarshall(), verifyMatchesOnly)
                        .DontMarshall();
        }

        protected abstract Task<List<MatchSong>> InternalGetSongsAsync(string title, string artist,
            int limit = 10);

        private async Task<List<MatchSong>> VerifyMatchesAsync(string title, string artist,
            IEnumerable<MatchSong> matches, bool verifyMatchesOnly = true)
        {
            var webSongs = matches?.OrderByDescending(p => p.Duration.Minutes).ToList();
            if (webSongs == null) return null;

            var sanitizedTitle = title.ToAudioticaSlug();
            var sanitizedArtist = artist.ToAudioticaSlug();

            // ReSharper disable once LoopCanBePartlyConvertedToQuery (code in this case looks more readable as it is)
            foreach (var webSong in webSongs)
            {
                var matchTitle = webSong.Title.ToAudioticaSlug();
                var matchArtist = webSong.Artist.ToAudioticaSlug();

                if (string.IsNullOrEmpty(matchTitle)) continue;

                // Run all type validators *VERY IMPORTANT*
                if (!_validators.All(songTypeValidator => songTypeValidator.IsValid(sanitizedTitle, matchTitle)))
                    continue;

                var isCorrectTitle = IsCorrectTitle(sanitizedTitle, sanitizedArtist, matchTitle, matchArtist);
                if (!isCorrectTitle) continue;

                // soundcloud/youtube doesnt have artist prop, check in title and author name (channel/username) for those cases
                var isCorrectArtist = matchArtist != null
                    ? matchArtist.Contains(sanitizedArtist) || sanitizedArtist.Contains(matchArtist)
                    : matchTitle.Contains(sanitizedArtist)
                      ||
                      (webSong.FileAuthor != null &&
                       webSong.FileAuthor.ToLower().Contains(sanitizedArtist.Replace(" ", "")));
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

            var mostUsedMinute =
                filterSongs.Where(p => !p.IsLinkDeath).GetMostUsedOccurrenceWhileIgnoringZero(p => p.Duration.Minutes);
            webSongs.Where(p => p.IsBestMatch).ForEach(p => p.IsBestMatch = p.Duration.Minutes == mostUsedMinute);

            return webSongs;
        }

        private bool IsCorrectTitle(string sanitizedTitle, string sanitizedArtist, string matchTitle, string matchArtist)
        {
            var titleDiff = Math.Abs(sanitizedTitle.Length - matchTitle.Length);
            var correct = (matchTitle.Contains(sanitizedTitle) || sanitizedTitle.Contains(matchTitle))
                          &&
                          titleDiff <=
                          MatchTitleLenghtThreshold + (string.IsNullOrEmpty(matchArtist) ? sanitizedArtist.Length : 0);

            if (correct) return true;

            // this is a workaround for songs that utilize "with {artist}" instead of "ft {artist}"
            var songHasFt = sanitizedTitle.Contains(" ft ");
            var matchHasFt = matchTitle.Contains(" ft ");

            if (songHasFt == matchHasFt) return false;

            if ((songHasFt || !sanitizedTitle.Contains(" with ")) && (matchHasFt || !matchTitle.Contains(" with ")))
                return false;

            sanitizedTitle = sanitizedTitle.Replace(" with ", " ft ");
            matchTitle = matchTitle.Replace(" with ", " ft ");
            correct = IsCorrectTitle(sanitizedTitle, sanitizedArtist, matchTitle, matchArtist);

            return correct;
        }

        private async Task<bool> CheckAudioSizeAsync(MatchSong matchSong)
        {
            using (var response = await matchSong.AudioUrl.ToUri().HeadAsync())
            {
                if (response == null || !response.IsSuccessStatusCode) return false;

                var type = response.Content.Headers.ContentType?.MediaType ??
                           response.Content.Headers.GetValues("Content-Type")?.FirstOrDefault() ?? "";
                if (!type.Contains("audio") && !type.Contains("octet-stream"))
                    return false;

                var size = response.Content.Headers.ContentLength;
                if (size != null)
                {
                    matchSong.ByteSize = (long) size;
                }
                return matchSong.ByteSize > 0;
            }
        }
    }
}