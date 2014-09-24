#region

using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;
using Audiotica.Data.Collection.SqlHelper;

#endregion

namespace Audiotica.Data.Collection.Model
{
    public class Album : BaseEntry
    {
        private BitmapImage _bitmap;

        public Album()
        {
            Songs = new List<Song>();
        }

        public string ProviderId { get; set; }

        [SqlProperty(ReferenceTo = typeof (Artist))]
        public long PrimaryArtistId { get; set; }


        public string Name { get; set; }

        public string SortName { get; set; }

        public string Genre { get; set; }

        public DateTime ReleaseDate { get; set; }

        [SqlIgnore]
        public List<Song> Songs { get; set; }

        [SqlIgnore]
        public Uri ArtworkUri { get; set; }

        [SqlIgnore]
        public BitmapImage Artwork
        {
            get
            {
                //don't want to load image every time, so we save the instance
                return _bitmap ?? (_bitmap = new BitmapImage(ArtworkUri));
            }
        }

        [SqlIgnore]
        public Artist PrimaryArtist { get; set; }
    }
}