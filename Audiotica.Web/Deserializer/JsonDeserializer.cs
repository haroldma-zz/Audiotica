using Newtonsoft.Json;

namespace Audiotica.Web.Deserializer
{
    public class JsonDeserializer : IDeserializer
    {
        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }

        public T Deserialize<T>(string content)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch
            {
                return default(T);
            }
        }
    }
}