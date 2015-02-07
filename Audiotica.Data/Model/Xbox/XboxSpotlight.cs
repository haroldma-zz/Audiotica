using System.Collections.Generic;

namespace Audiotica.Data.Model.Xbox
{
    public class XboxSpotlight
    {
        public string ContentListId { get; set; }

        public List<XboxFeature> Items { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string BackgroundImageUrl { get; set; }

        public string HeroImageUrl { get; set; }
    }
}