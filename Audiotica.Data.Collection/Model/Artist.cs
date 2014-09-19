using System.Collections.Generic;

namespace Audiotica.Data.Collection.Model
{
    public class Artist
    {
        public Artist()
        {
            Songs = new List<Song>();
            Albums = new List<Album>();
        }

        public long Id { get; set; }

        public string XboxId { get; set; }

        public string LastFmId { get; set; }

        public string Name { get; set; }

        public List<Song> Songs { get; set; } 
        
        public List<Album> Albums { get; set; } 
    }
}
