using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using SQLite;

namespace Audiotica.Collection.Model
{
    public class Album : BaseDbEntry
    {
        [Indexed]
        public string XboxId { get; set; }

        [Indexed]
        public string LastFmId { get; set; }

        [Indexed]
        public int PrimaryArtistId { get; set; }


        public string Name { get; set; }

        public string Genre { get; set; }

        public DateTime ReleaseDate { get; set; }

        [Ignore]
        public List<Song> Songs { get; set; } 

        [Ignore]
        public BitmapImage Artwork { get; set; }

        [Ignore]
        public Artist PrimaryArtist { get; set; }
    }
}
