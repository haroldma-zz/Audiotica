using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Audiotica.Core.Helpers;
using Audiotica.Core.Utilities;

namespace Audiotica.Core.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        ///     Slugifies the text using the Audiotica algorith.
        ///     Used to compared song titles/artists without worrying about unimportant variations.
        /// </summary>
        /// <example>
        ///     before:
        ///     Skrillex and Diplo - Where Are Ü Now (with Justin Bieber)
        ///     after:
        ///     skrillex diplo where are u now with justin bieber
        /// </example>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string ToAudioticaSlug(this string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            var str = WebUtility.HtmlDecode(text.ToLower());
            str = str.Replace(" and ", " ").Replace("feat.", "ft");

            str = str.ToUnaccentedText();
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();

            return str;
        }

        public static string ToUnaccentedText(this string accentedString)
        {
            return string.IsNullOrEmpty(accentedString) ? accentedString : DiacritisHelper.Remove(accentedString);
        }

        public static string ToSanitizedFileName(this string str, string invalidMessage)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            if (str.Length > 35)
            {
                str = str.Substring(0, 35);
            }

            str = str.ToValidFileNameEnding();

            /*
             * A filename cannot contain any of the following characters:
             * \ / : * ? " < > |
             */
            var name =
                str.Replace("\\", string.Empty)
                    .Replace("/", string.Empty)
                    .Replace(":", " ")
                    .Replace("*", string.Empty)
                    .Replace("?", string.Empty)
                    .Replace("\"", "'")
                    .Replace("<", string.Empty)
                    .Replace(">", string.Empty)
                    .Replace("|", " ");

            return string.IsNullOrEmpty(name) ? invalidMessage : name;
        }

        public static string ToValidFileNameEnding(this string str)
        {
            var isNonAccepted = true;

            while (isNonAccepted)
            {
                var lastChar = str[str.Length - 1];

                isNonAccepted = lastChar == ' ' || lastChar == '.' || lastChar == ';' || lastChar == ':';

                if (isNonAccepted) str = str.Remove(str.Length - 1);
                else break;

                if (str.Length == 0) return str;

                isNonAccepted = lastChar == ' ' || lastChar == '.' || lastChar == ';' || lastChar == ':';
            }

            return str;
        }

        public static string ToHtmlStrippedText(this string str)
        {
            var array = new char[str.Length];
            var arrayIndex = 0;
            var inside = false;

            foreach (var o in str.ToCharArray())
            {
                switch (o)
                {
                    case '<':
                        inside = true;
                        continue;
                    case '>':
                        inside = false;
                        continue;
                }
                if (inside) continue;

                array[arrayIndex] = o;
                arrayIndex++;
            }
            return new string(array, 0, arrayIndex);
        }

        public static string Append(this string left, string right) => left + " " + right;

        public static Uri ToUri(this string url, UriKind kind = UriKind.Absolute)
        {
            return new Uri(url, kind);
        }
    }
}