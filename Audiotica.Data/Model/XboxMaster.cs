using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audiotica.Data.Model
{
    /// <summary>
    /// the public api does not contain some stuff the app needs.
    /// but that's nothing fiddler can't fix.  
    /// Bellow all the data models from the undocumented one.
    /// </summary>
    public class XboxItem
    {
        public string ItemType { get; set; }
        public string ItemId { get; set; }
        public string BingId { get; set; }
        public int Rank { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string LongDescription { get; set; }
        public string ImageUrl { get; set; }
        public string PrimaryArtistId { get; set; }
        public int TrackCount { get; set; }
        public bool IsExplicit { get; set; }
        public string ImageId { get; set; }
        public string Genre { get; set; }
        public string SubGenre { get; set; }
        public string Duration { get; set; }
    }

    public class XboxFeedRoot
    {
        public string Title { get; set; }
        public List<XboxItem> Items { get; set; }
        public string ContentListId { get; set; }
        public string CreationDate { get; set; }
    }
}
