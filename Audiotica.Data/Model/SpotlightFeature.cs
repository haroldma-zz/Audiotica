using System.Collections.Generic;

namespace Audiotica.Data.Model
{
    public class SpotlightFeature
    {
        public string Action { get; set; }

        public string ImageUri { get; set; }

        public ShowTo ShowTo { get; set; }

        public string Text { get; set; }

        public string Title { get; set; }

        public bool InsertAtTop { get; set; }

        public bool ShowToNot { get; set; }
    }

    public enum ShowTo
    {
        All, 

        Cloud, 

        ActiveSubscription, 

        Trial, 

        Cancelled, 

        PastDue, 

        Beta, 

        ScrobblingEnabled
    }

    public class AudioticaSpotlight
    {
        public List<SpotlightFeature> LargeFeatures { get; set; }

        public List<SpotlightFeature> MediumFeatures { get; set; }
    }
}