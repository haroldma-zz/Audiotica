using HtmlAgilityPack;

namespace Audiotica.Web.Deserializer
{
    public class HtmlDeserializer : IDeserializer
    {
        public HtmlDeserializer()
        {
            if (HtmlNode.ElementsFlags.ContainsKey("form"))
                HtmlNode.ElementsFlags.Remove("form");
        }

        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }

        public T Deserialize<T>(string content)
        {
            if (typeof (T) != typeof (HtmlDocument)) return default(T);

            var document = new HtmlDocument();
            document.LoadHtml(content);
            return (T) (object) document;
        }
    }
}