using System;
using System.Linq;
using Audiotica.Web.Models;

namespace Audiotica.Web.Extensions
{
    public static class MatchSongExtensions
    {
        /// <summary>
        ///     Sets the name and artist by extracting it from a title that uses the specified [seperator].
        /// </summary>
        /// <param name="song">The song.</param>
        /// <param name="title">The title.</param>
        /// <param name="artistOnLeft">if set to <c>true</c> [artist] will be taken from the left side.</param>
        /// <param name="seperator">The seperator that the title is using.</param>
        /// <example>
        ///     With [artistOnLeft] set to <c>true</c>
        ///     "Maroon 5 - Maps"
        ///     song.Title = Maps
        ///     song.Artist = Maroon 5
        /// </example>
        public static void SetNameAndArtistFromTitle(this MatchSong song, string title, bool artistOnLeft,
            char seperator = '-')
        {
            var titleSplit = title.Split(seperator).Select(p => p.Trim()).ToArray();
            if (titleSplit.Length < 1) return;

            var left = titleSplit[0];
            var right = string.Join($" {seperator} ", titleSplit.Skip(1));


            if (string.IsNullOrEmpty(right))
            {
                song.Title = left;
            }
            else if (artistOnLeft)
            {
                song.Artist = left;
                song.Title = right;
            }
            else
            {
                song.Artist = right;
                song.Title = left;
            }
        }
    }
}