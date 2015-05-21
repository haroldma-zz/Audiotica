using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Audiotica.Web.Deserializer
{
    public static class DeserializerExtensions
    {
        public static T Deserialize<T>(this IDeserializer deserializer, HttpWebResponse response)
        {
            string content;

            using (var stream = response.GetResponseStream())
            {
                var reader = new StreamReader(stream);
                content = reader.ReadToEnd();
            }

            return deserializer.Deserialize<T>(content);
        }

        public static async Task<T> Deserialize<T>(this IDeserializer deserializer, HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return deserializer.Deserialize<T>(content);
        }
    }
}