using System;
using System.Collections.ObjectModel;
using Audiotica.Core.Common;
using Newtonsoft.Json;
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

        public Artist(CloudArtist cloud) : this()
        {
            Name = cloud.Name;
            ProviderId = cloud.ProviderId;
            CloudId = cloud.Id;
        }

        public string ProviderId { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public bool HasArtwork { get; set; }

        [JsonIgnore]
        public string CloudId { get; set; }

        [JsonIgnore]
        public DateTime LastUpdated { get; set; }

        [Ignore]
        [JsonIgnore]
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
        [JsonIgnore]
        public ObservableCollection<Song> Songs { get; set; }

        [Ignore]
        [JsonIgnore]
        public ObservableCollection<Album> Albums { get; set; }

        [Ignore]
        [JsonIgnore]
        public ObservableCollection<AddableCollectionItem> AddableTo { get; set; }
    }
}