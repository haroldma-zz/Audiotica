using System;

namespace Audiotica.Web.Models
{
    public class WebItem
    {
        public WebItem()
        {
        }

        public WebItem(Type provider)
        {
            MetadataProvider = provider;
        }

        public Type MetadataProvider { get; set; }
        public bool IsPartial { get; set; }
        public string Token { get; set; }
    }
}