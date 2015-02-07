using System.Collections.Generic;

namespace Audiotica.Data.Model
{
    public class EntryBase
    {
        /// <summary>
        /// Identifier for this piece of content. All IDs are of the form {namespace}.{actual identifier} 
        /// and may be used in lookup requests.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of this piece of content.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A direct link to the default image associated with this piece of content.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// A music.xbox.com link that redirects to a contextual page for this piece of content on the 
        /// relevant Xbox Music client application depending on the user's device or operating system.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// An optional collection of other IDs that identify this piece of content on top of the main ID. 
        /// Each key is the namespace or subnamespace in which the ID belongs, and each value is a secondary ID for this piece of content.
        /// </summary>
        public Dictionary<string, string> OtherIds { get; set; }

        /// <summary>
        /// An indication of the data source for this piece of content. Possible values are Collection and Catalog.
        /// </summary>
        public string Source { get; set; }
    }
}