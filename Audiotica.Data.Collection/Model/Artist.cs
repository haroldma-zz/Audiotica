using System.Collections.ObjectModel;
using Audiotica.Core.Common;
using SQLite;

namespace Audiotica.Data.Collection.Model
{
    public class Artist : BaseEntry
    {
        private IBitmapImage _artwork;

        public Artist()
        {
            Songs = new ObservableCollection<Song>();
            Albums = new ObservableCollection<Album>();
            AddableTo = new ObservableCollection<AddableCollectionItem>();
        }

        public string ProviderId { get; set; }
        public string Name { get; set; }
        public bool HasArtwork { get; set; }

        [Ignore]
        public IBitmapImage Artwork
        {
            get { return _artwork; }
            set
            {
                _artwork = value;
                OnPropertyChanged();
            }
        }

        [Ignore]
        public ObservableCollection<Song> Songs { get; set; }

        [Ignore]
        public ObservableCollection<Album> Albums { get; set; }

        [Ignore]
        public ObservableCollection<AddableCollectionItem> AddableTo { get; set; }
    }
}