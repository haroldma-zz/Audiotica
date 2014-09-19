#region

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Audiotica.Collection;
using Audiotica.Data.Collection.Model;

#endregion

namespace Audiotica.Data.Collection.DesignTime
{
    public class DesignCollectionService : ICollectionService
    {
        public ObservableCollection<Song> Songs { get; set; }
        public ObservableCollection<Album> Albums { get; set; }
        public ObservableCollection<Artist> Artists { get; set; }

        public void LoadLibrary()
        {
            Songs = new ObservableCollection<Song>
            {
                new Song
                {
                    Name = "Maps",
                    Artist = new Artist {Name = "Maroon 5"},
                    Album =
                        new Album
                        {
                            Name = "V",
                            Artwork =
                                new BitmapImage(
                                    new Uri("http://static.musictoday.com/store/bands/93/product_medium/IXDDM501.JPG"))
                        }
                },
                new Song
                {
                    Name = "Animal",
                    Artist = new Artist {Name = "Maroon 5"},
                    Album =
                        new Album
                        {
                            Name = "V",
                            Artwork =
                                new BitmapImage(
                                    new Uri("http://static.musictoday.com/store/bands/93/product_medium/IXDDM501.JPG"))
                        }
                }
            };
        }

        public Task LoadLibraryAsync()
        {
            throw new NotImplementedException();
        }

        public Task AddSongAsync(Song song, string artworkUrl)
        {
            throw new NotImplementedException();
        }

        public Task DeleteSongAsync(Song song)
        {
            throw new NotImplementedException();
        }
    }
}