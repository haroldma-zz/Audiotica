using System;
using Audiotica.Core.Common;

namespace Audiotica.Web.Models
{
    public class WebArtist : WebItem, IConvertibleObject
    {
        public WebArtist(Type provider) : base(provider)
        {
        }

        public string Name { get; set; }
        public Uri Artwork { get; set; }
        public object PreviousConversion { get; set; }
    }
}