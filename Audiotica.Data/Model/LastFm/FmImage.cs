#region

using Newtonsoft.Json;

#endregion

namespace Audiotica.Data.Model.LastFm
{
    public class FmImage
    {
        [JsonProperty(PropertyName = "#text")]
        public string text { get; set; }

        public string size { get; set; }
    }
}