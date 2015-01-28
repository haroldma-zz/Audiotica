using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Audiotica.Core.Utils
{
    public static class StringExtensions
    {
        public static string CleanForFileName(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            /*
             * A filename cannot contain any of the following characters:
             * \ / : * ? " < > |
             */
            return
                str.Replace("\\", string.Empty)
                    .Replace("/", string.Empty)
                    .Replace(":", " ")
                    .Replace("*", string.Empty)
                    .Replace("?", string.Empty)
                    .Replace("\"", "'")
                    .Replace("<", string.Empty)
                    .Replace(">", string.Empty)
                    .Replace("|", " ");
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