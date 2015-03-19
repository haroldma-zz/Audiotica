#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Data.Collection.Model;
using TagLib;

#endregion

namespace Audiotica.Data.Collection.DesignTime
{
    public class DesignCollectionService : ICollectionService
    {
        private readonly IBitmapFactory _bitmapFactory;

        public DesignCollectionService(IBitmapFactory bitmapFactory)
        {
            _bitmapFactory = bitmapFactory;
            LoadLibrary();
        }

        public bool IsLibraryLoaded { get; private set; }
        public event EventHandler LibraryLoaded;
        public int ScaledImageSize { get; set; }
        public OptimizedObservableCollection<Song> Songs { get; set; }

        public OptimizedObservableCollection<Album> Albums { get; set; }

        public OptimizedObservableCollection<Artist> Artists { get; set; }

        public OptimizedObservableCollection<RadioStation> Stations { get; set; }

        public OptimizedObservableCollection<Playlist> Playlists { get; set; }
        public OptimizedObservableCollection<QueueSong> PlaybackQueue { get; private set; }
        public OptimizedObservableCollection<QueueSong> ShufflePlaybackQueue { get; private set; }
        public OptimizedObservableCollection<QueueSong> CurrentPlaybackQueue { get; private set; }

        public void LoadLibrary()
        {
            Songs = new OptimizedObservableCollection<Song>
            {
                new Song
                {
                    Name = "Maps",
                    Artist =
                        new Artist
                        {
                            Name = "Maroon 5",
                            Artwork =
                                _bitmapFactory.CreateImage(
                                    new Uri(
                                        "http://musicimage.xboxlive.com/content/music.1F154700-0200-11DB-89CA-0019B92A3933/image?locale=en-US"))
                        },
                    Album =
                        new Album
                        {
                            Name = "V",
                            Artwork =
                                _bitmapFactory.CreateImage(
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
                                _bitmapFactory.CreateImage(
                                    new Uri("http://static.musictoday.com/store/bands/93/product_medium/IXDDM501.JPG"))
                        }
                }
            };

            Albums = new OptimizedObservableCollection<Album>
            {
                new Album
                {
                    Id = 0,
                    Name = "V",
                    Artwork =
                        _bitmapFactory.CreateImage(
                            new Uri("http://static.musictoday.com/store/bands/93/product_medium/IXDDM501.JPG")),
                    PrimaryArtist = new Artist {Name = "Maroon 5"},
                    Genre = "Pop",
                    Songs = new OptimizedObservableCollection<Song>(Songs)
                }
            };

            Artists = new OptimizedObservableCollection<Artist>
            {
                new Artist {Name = "Maroon 5", Albums = Albums, Songs = Songs},
                new Artist {Name = "Taylor Swift"}
            };

            Playlists = new OptimizedObservableCollection<Playlist>
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

            PlaybackQueue = new OptimizedObservableCollection<QueueSong>(pSongs);
        }

        public Task LoadLibraryAsync()
        {
            throw new NotImplementedException();
        }

        public Task AddSongAsync(Song song, Tag tags)
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

        public Task AddStationAsync(RadioStation station)
        {
            throw new NotImplementedException();
        }

        public Task DeleteStationAsync(RadioStation station)
        {
            throw new NotImplementedException();
        }

        public void ShuffleModeChanged()
        {
            throw new NotImplementedException();
        }

        public Task ShuffleCurrentQueueAsync()
        {
            throw new NotImplementedException();
        }

        public Task ClearQueueAsync()
        {
            throw new NotImplementedException();
        }

        public Task<QueueSong> AddToQueueAsync(Song song, QueueSong position = null, bool shuffleInsert = true)
        {
            throw new NotImplementedException();
        }

        public Task MoveQueueFromToAsync(int oldIndex, int newIndex)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFromQueueAsync(QueueSong songToRemove)
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