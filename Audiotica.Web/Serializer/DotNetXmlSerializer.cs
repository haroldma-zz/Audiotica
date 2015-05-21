using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Audiotica.Web.Serializer
{
    /// <summary>
    ///     Wrapper for System.Xml.Serialization.XmlSerializer.
    /// </summary>
    public class DotNetXmlSerializer : ISerializer
    {
        /// <summary>
        ///     Default constructor, does not specify namespace
        /// </summary>
        public DotNetXmlSerializer()
        {
            ContentType = "application/xml";
            Encoding = Encoding.UTF8;
        }

        /// <summary>
        ///     Specify the namespaced to be used when serializing
        /// </summary>
        /// <param name="namespace">XML namespace</param>
        public DotNetXmlSerializer(string @namespace) : this()
        {
            Namespace = @namespace;
        }

        /// <summary>
        ///     Encoding for serialized content
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        ///     Serialize the object as XML
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <param name="clrPropertyNameToLower">if set to <c>true</c> [color property name to lower].</param>
        /// <returns>
        ///     XML as string
        /// </returns>
        public string Serialize(object obj, bool clrPropertyNameToLower = false)
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, Namespace);
            var serializer = new XmlSerializer(obj.GetType());
            var writer = new EncodingStringWriter(Encoding);
            serializer.Serialize(writer, obj, ns);

            return writer.ToString();
        }

        /// <summary>
        ///     Name of the root element to use when serializing
        /// </summary>
        public string RootElement { get; set; }

        /// <summary>
        ///     XML namespace to use when serializing
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        ///     Format string to use when serializing dates
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        ///     Content type for serialized content
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        ///     Need to subclass StringWriter in order to override Encoding
        /// </summary>
        private class EncodingStringWriter : StringWriter
        {
            public EncodingStringWriter(Encoding encoding)
            {
                Encoding = encoding;
            }

            public override Encoding Encoding { get; }
        }
    }
}