using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;

namespace Audiotica.Data.Collection.Model
{
    public class Album
    {
        public Album()
        {
            Songs = new List<Song>();
        }

        public long Id { get; set; }

        public string XboxId { get; set; }

        public string LastFmId { get; set; }

        public long PrimaryArtistId { get; set; }


        public string Name { get; set; }

        public string Genre { get; set; }

        public DateTime ReleaseDate { get; set; }

        public List<Song> Songs { get; set; } 

        public BitmapImage Artwork { get; set; }

        public Artist PrimaryArtist { get; set; }
    }
}
