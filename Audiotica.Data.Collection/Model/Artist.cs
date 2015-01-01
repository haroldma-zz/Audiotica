using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;
using Audiotica.Data.Collection.SqlHelper;

namespace Audiotica.Data.Collection.Model
{
    public class Artist : BaseEntry
    {
        private BitmapImage _artwork;

        public Artist()
        {
            Songs = new ObservableCollection<Song>();
            Albums = new ObservableCollection<Album>();
        }

        public string ProviderId { get; set; }

        public string Name { get; set; }

        public bool HasArtwork { get; set; }

        public BitmapImage Artwork
        {
            get { return _artwork; }
            set { Set(ref _artwork, value); }
        }

        public ObservableCollection<Song> Songs { get; set; }

        public ObservableCollection<Album> Albums { get; set; } 
    }
}
