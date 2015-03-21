using System.Globalization;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Audiotica.Core.Utils
{
    public static class StringExtensions
    {
        public static string RemoveAccents(this string accentedStr)
        {
            byte[] tempBytes = Encoding.GetEncoding("ISO-8859-8").GetBytes(accentedStr);
            return Encoding.UTF8.GetString(tempBytes, 0, tempBytes.Length);
        }

        public static string CleanForFileName(this string str, string invalidMessage)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            if (str.Length > 35)
            {
                str = str.Substring(0, 35);
            }

            str = str.ForceValidEnding();

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

        public static string ForceValidEnding(this string str)
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

        public static async Task<T> DeserializeAsync<T>(this string json)
        {
            return await Task.Factory.StartNew(
                () =>
                    {
                        try
                        {
                            return JsonConvert.DeserializeObject<T>(json);
                        }
                        catch
                        {
                            return default(T);
                        }
                    }).ConfigureAwait(false);
        }

        public static string StripHtmlTags(this string str)
        {
            return HtmlRemoval.StripTagsRegex(str);
        }
    }
}