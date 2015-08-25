using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using System.Text.RegularExpressions;
using Audiotica.Core.Helpers;
using Audiotica.Windows.Enums;
using Newtonsoft.Json;

namespace Audiotica.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool EqualsIgnoreCase(this string text, string other) =>
            text.Equals(other, StringComparison.CurrentCultureIgnoreCase);

        public static string GetEnumText(this Enum value)
        {
            var fi = value.GetType().GetRuntimeField(value.ToString());

            var attributes =
                (TextAttribute[]) fi.GetCustomAttributes(typeof (TextAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Text;
            return value.ToString();
        }

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
            str = str.Replace(" and ", " ").Replace("feat", "ft");

            str = str.ToUnaccentedText();
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();

            return str;
        }

        /// <summary>
        ///     Tokenizes the specified values.
        ///     Simple array serializing.
        ///     ["test", "yo testing"] =&gt; "test yo%20testing"
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public static string Tokenize(this string[] values)
        {
            // the delimiter used for tokenizing is a space, let's encode it.
            // Encode % and then we can encode space, without worring if the original string had %.
            var encoded = values.Select(p => p?.Replace("%", "%25").Replace(" ", "%20"));

            // Join using the space delimiter
            return string.Join(" ", encoded);
        }

        /// <summary>
        ///     DeTokenizes the specified values.
        ///     "test yo%20testing" =&gt; ["test", "yo testing"]
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public static string[] DeTokenize(this string token)
        {
            // reverse the proccess of encoding
            var values = token.Split(' ');
            return values.Select(p => p?.Replace("%20", " ").Replace("%25", "%")).ToArray();
        }

        public static bool IsAnyNullOrEmpty(params string[] values) => values.Any(string.IsNullOrEmpty);

        public static T TryDeserializeJson<T>(this string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                return default(T);
            }
        }

        public static object TryDeserializeJsonWithTypeInfo(this string json)
        {
            if (string.IsNullOrEmpty(json)) return json;

            try
            {
                return JsonConvert.DeserializeObject(json, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
                });
            }
            catch
            {
                return null;
            }
        }

        public static string SerializeToJsonWithTypeInfo(this object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
                });
            }
            catch
            {
                return null;
            }
        }

        public static string SerializeToJson(this object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj);
            }
            catch
            {
                return null;
            }
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