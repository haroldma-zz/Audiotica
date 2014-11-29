using System.Collections.Generic;
using System.Collections.ObjectModel;
using Audiotica.Data.Collection.SqlHelper;

namespace Audiotica.Data.Collection.Model
{
    public class Artist : BaseEntry
    {
        public Artist()
        {
            Songs = new ObservableCollection<Song>();
            Albums = new ObservableCollection<Album>();
        }

        public string ProviderId { get; set; }

        public string Name { get; set; }

        public ObservableCollection<Song> Songs { get; set; }

        public ObservableCollection<Album> Albums { get; set; } 
    }
}
