using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Audiotica.Collection.Model
{
    public class Artist : BaseDbEntry
    {
        public string Name { get; set; }

        [Ignore]
        public List<Song> Songs { get; set; } 
        
        [Ignore]
        public List<Album> Albums { get; set; } 
    }
}
