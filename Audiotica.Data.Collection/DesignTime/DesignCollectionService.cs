#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Audiotica.Data.Collection.Model;

#endregion

namespace Audiotica.Data.Collection.DesignTime
{
    public class DesignCollectionService : ICollectionService
    {
        public DesignCollectionService()
        {
            LoadLibrary();
        }

        public bool IsLibraryLoaded { get; private set; }
        public event EventHandler LibraryLoaded;
        public ObservableCollection<Song> Songs { get; set; }
        public ObservableCollection<Album> Albums { get; set; }
        public ObservableCollection<Artist> Artists { get; set; }
        public ObservableCollection<Playlist> Playlists { get; set; }

        public ObservableCollection<QueueSong> PlaybackQueue { get; private set; }

        public void LoadLibrary(bool loadEssentials = true)
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
                                    new BitmapImage(new Uri("http://static.musictoday.com/store/bands/93/product_medium/IXDDM501.JPG"))
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
                                   new BitmapImage(new Uri("http://static.musictoday.com/store/bands/93/product_medium/IXDDM501.JPG"))
                        }
                }
            };

            Albums = new ObservableCollection<Album>
            {
                 new Album
                        {
                            Id= 0,
                            Name = "V",
                            Artwork =
                                    new BitmapImage(new Uri("http://static.musictoday.com/store/bands/93/product_medium/IXDDM501.JPG")),
                            PrimaryArtist = new Artist { Name = "Maroon 5"},
                            Genre = "Pop",
                            Songs = Songs
                        }
            };

            Artists = new ObservableCollection<Artist>
            {
                new Artist {Name = "Maroon 5", Albums = Albums, Songs = Songs},
                new Artist {Name = "Taylor Swift"},
            };

            Playlists = new ObservableCollection<Playlist>
            {
                new Playlist
                {
                    Name = "Fav 5"
                },
                new Playlist
                {
                    Name = "workout fun!"
                }
            };

            var pSongs = new ObservableCollection<PlaylistSong>();
            foreach (var song in Songs)
            {
                pSongs.Add(new PlaylistSong
                {
                    Song = song
                });
            }

            foreach (var playlist in Playlists)
            {
                playlist.Songs = pSongs;
            }

            PlaybackQueue = new ObservableCollection<QueueSong>(pSongs);
        }

        public Task LoadLibraryAsync(bool loadEssentials = true)
        {
            throw new NotImplementedException();
        }

        public Task AddSongAsync(Song song, string artworkUrl, string artistArtwork)
        {
            throw new NotImplementedException();
        }

        public Task AddSongAsync(Song song, StorageFile songFile, string artistArtwork)
        {
            throw new NotImplementedException();
        }

        public Task DeleteSongAsync(Song song)
        {
            throw new NotImplementedException();
        }

        public Task<List<HistoryEntry>> FetchHistoryAsync()
        {
            throw new NotImplementedException();
        }

        public bool SongAlreadyExists(string providerId, string name, string album, string artist)
        {
            throw new NotImplementedException();
        }

        public bool SongAlreadyExists(string localSongPath)
        {
            throw new NotImplementedException();
        }

        public Task ClearQueueAsync()
        {
            throw new NotImplementedException();
        }

        public Task<QueueSong> AddToQueueAsync(Song song, int position = -1)
        {
            throw new NotImplementedException();
        }

        public Task MoveQueueFromToAsync(int oldIndex, int newIndex)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFromQueueAsync(Song songToRemove)
        {
            throw new NotImplementedException();
        }

        public Task<Playlist> CreatePlaylistAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task DeletePlaylistAsync(Playlist playlist)
        {
            throw new NotImplementedException();
        }

        public Task AddToPlaylistAsync(Playlist playlist, Song song)
        {
            throw new NotImplementedException();
        }

        public Task MovePlaylistFromToAsync(Playlist playlist, int oldIndex, int newIndex)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFromPlaylistAsync(Playlist playlist, PlaylistSong songToRemove)
        {
            throw new NotImplementedException();
        }
    }
}