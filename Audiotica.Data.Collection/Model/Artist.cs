using System.Collections.Generic;
using Audiotica.Data.Collection.SqlHelper;

namespace Audiotica.Data.Collection.Model
{
    public class Artist : BaseEntry
    {
        public Artist()
        {
            Songs = new List<Song>();
            Albums = new List<Album>();
        }

        public string ProviderId { get; set; }

        public string Name { get; set; }

        public string SortName { get; set; }

        [SqlIgnore]
        public List<Song> Songs { get; set; }
        [SqlIgnore]
        public List<Album> Albums { get; set; } 
    }
}
