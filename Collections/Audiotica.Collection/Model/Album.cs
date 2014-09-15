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
        public string Name { get; set; }

        public string Genre { get; set; }

        public string ReleaseDate { get; set; }

        [Ignore]
        public List<Song> Songs { get; set; } 

        [Ignore]
        public BitmapImage Artwork { get; set; }
    }
}
