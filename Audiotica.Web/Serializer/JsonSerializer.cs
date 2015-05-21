using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Audiotica.Web.Serializer
{
    public class JsonSerializer : ISerializer
    {
        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
        public string ContentType { get; set; } = "application/json";
        public string Serialize(object obj, bool clrPropertyNameToLower = false)
        {
            if (clrPropertyNameToLower)
                return JsonConvert.SerializeObject(obj,
                    new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()});
            return JsonConvert.SerializeObject(obj);
        }
    }
}