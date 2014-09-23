using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;
using Audiotica.Data.Collection.SqlHelper;

namespace Audiotica.Data.Collection.Model
{
    public class Album : BaseEntry
    {
        public Album()
        {
            Songs = new List<Song>();
        }

        public string ProviderId { get; set; }

        [SqlProperty(ReferenceTo = typeof(Artist))]
        public long PrimaryArtistId { get; set; }


        public string Name { get; set; }

        public string Genre { get; set; }

        public DateTime ReleaseDate { get; set; }

        [SqlIgnore]
        public List<Song> Songs { get; set; }

        [SqlIgnore]
        public Uri Artwork { get; set; }

        [SqlIgnore]
        public Artist PrimaryArtist { get; set; }
    }
}
