#region

using System.Threading.Tasks;
using Newtonsoft.Json;

#endregion

namespace Audiotica.Core.Utilities
{
    public static class StringExtensions
    {
        public static async Task<T> DeserializeAsync<T>(this string json)
        {
            return await Task.Factory.StartNew(() =>
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

        public static string FromLanguageResource(this string str)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            return loader.GetString(str);
        }
    }
}