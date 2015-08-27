using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Audiotica.Web.Deserializer
{
    /// <summary>
    ///     Wrapper for System.Xml.Serialization.XmlSerializer.
    /// </summary>
    public class DotNetXmlDeserializer : IDeserializer
    {
        public string DateFormat { get; set; }
        public string Namespace { get; set; }
        public string RootElement { get; set; }

        public T Deserialize<T>(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return default(T);
            }

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                var serializer = new XmlSerializer(typeof (T));
                return (T) serializer.Deserialize(stream);
            }
        }
    }
}