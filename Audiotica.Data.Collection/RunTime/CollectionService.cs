﻿#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection.Model;

#endregion

namespace Audiotica.Data.Collection.RunTime
{
    public class CollectionService : ICollectionService
    {
        private readonly CoreDispatcher _dispatcher;
        private readonly Dictionary<long, QueueSong> _lookupMap = new Dictionary<long, QueueSong>();
        private readonly ISqlService _sqlService;

        public CollectionService(ISqlService sqlService, CoreDispatcher dispatcher)
        {
            _sqlService = sqlService;
            _dispatcher = dispatcher;
            Songs = new ObservableCollection<Song>();
            Artists = new ObservableCollection<Artist>();
            Albums = new ObservableCollection<Album>();
            Playlists = new ObservableCollection<Playlist>();
            PlaybackQueue = new ObservableCollection<QueueSong>();
        }


        public ObservableCollection<Song> Songs { get; set; }
        public ObservableCollection<Album> Albums { get; set; }
        public ObservableCollection<Artist> Artists { get; set; }
        public ObservableCollection<Playlist> Playlists { get; set; }

        public ObservableCollection<QueueSong> PlaybackQueue { get; private set; }

        public void LoadLibrary()
        {
            var songs = _sqlService.SelectAll<Song>().OrderByDescending(p => p.Id);
            var albums = _sqlService.SelectAll<Album>().OrderByDescending(p => p.Id);
            var artists = _sqlService.SelectAll<Artist>().OrderByDescending(p => p.Id);

            foreach (var song in songs)
            {
                song.Artist = artists.FirstOrDefault(p => p.Id == song.ArtistId);
                song.Album = albums.FirstOrDefault(p => p.Id == song.AlbumId);
            }

            foreach (var album in albums)
            {
                album.Songs.AddRange(songs.Where(p => p.AlbumId == album.Id).OrderBy(p => p.TrackNumber));
                album.PrimaryArtist = artists.FirstOrDefault(p => p.Id == album.PrimaryArtistId);

                if (_dispatcher != null)
                    _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        async () => album.Artwork = await GetArtworkAsync(album.Id)).AsTask().Wait();
            }

            foreach (var artist in artists)
            {
                artist.Songs.AddRange(songs.Where(p => p.ArtistId == artist.Id));
                artist.Albums.AddRange(albums.Where(p => p.PrimaryArtistId == artist.Id));
            }

            //Foreground app
            if (_dispatcher != null)
            {
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Songs.AddRange(songs);
                    Artists.AddRange(artists);
                    Albums.AddRange(albums);
                }).AsTask().Wait();
            }

                //background player
            else
            {
                Songs = new ObservableCollection<Song>(songs);
                Artists = new ObservableCollection<Artist>(artists);
                Albums = new ObservableCollection<Album>(albums);
            }

            LoadQueue();
            LoadPlaylists();
            CleanupFiles();
        }

        public Task LoadLibraryAsync()
        {
            //just return non async as a task
            return Task.Factory.StartNew(LoadLibrary);
        }

        public async Task AddSongAsync(Song song, string artworkUrl)
        {
            if (Songs.Count(p => p.ProviderId == song.ProviderId) > 0)
                throw new Exception("AlreadySavedToast".FromLanguageResource());

            #region create artist

            if (song.Artist.ProviderId == "lastid.")
                song.Artist.ProviderId = "autc.single." + song.ProviderId;

            var artist = Artists.FirstOrDefault(entry => entry.ProviderId == song.Artist.ProviderId);

            if (artist == null)
            {
                await _sqlService.InsertAsync(song.Artist);

                if (song.Album != null)
                    song.Album.PrimaryArtistId = song.Artist.Id;
                Artists.Insert(0, song.Artist);
            }

            else
            {
                song.Artist = artist;

                if (song.Album != null)
                    song.Album.PrimaryArtistId = artist.Id;
            }
            song.ArtistId = song.Artist.Id;

            #endregion

            #region create album

            if (song.Album == null)
            {
                song.Album = new Album
                {
                    PrimaryArtistId = song.ArtistId,
                    Name = song.Name + " (Single)",
                    PrimaryArtist = song.Artist,
                    ProviderId = "autc.single." + song.ProviderId
                };
                await _sqlService.InsertAsync(song.Album);
                Albums.Insert(0, song.Album);
                song.Artist.Albums.Insert(0, song.Album);
            }
            else
            {
                var album = Albums.FirstOrDefault(p => p.ProviderId == song.Album.ProviderId);

                if (album != null)
                    song.Album = album;
                else
                {
                    await _sqlService.InsertAsync(song.Album);
                    Albums.Insert(0, song.Album);
                    song.Artist.Albums.Insert(0, song.Album);
                }
            }

            song.AlbumId = song.Album.Id;

            #endregion

            #region Download artwork

            if (artworkUrl != null)
            {
//Use the album if one is available
                var filePath = string.Format(CollectionConstant.ArtworkPath, song.Album.Id);

                //Check if the album artwork has already been downloaded
                var artworkExists = await StorageHelper.FileExistsAsync(filePath);

                if (!artworkExists)
                {
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            using (var stream = await client.GetStreamAsync(artworkUrl))
                            {
                                using (
                                    var fileStream =
                                        await
                                            (await StorageHelper.CreateFileAsync(filePath)).OpenStreamForWriteAsync()
                                    )
                                {
                                    await stream.CopyToAsync(fileStream);
                                    //now set it
                                    song.Album.Artwork =
                                        new BitmapImage(new Uri(CollectionConstant.LocalStorageAppPath + filePath));
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Some shit happened saving the artwork, here: " + e);
                    }
                }
            }

            if (song.Album.Artwork == null)
                song.Album.Artwork = CollectionConstant.MissingArtworkImage;

            #endregion

            //Insert to db
            await _sqlService.InsertAsync(song);

            song.Artist.Songs.Insert(0, song);
            song.Album.Songs.Insert(0, song);
            Songs.Insert(0, song);
        }

        public async Task DeleteSongAsync(Song song)
        {
            // remove it from artist, albums and playlists songs
            var artist = Artists.FirstOrDefault(p => p.Songs.Contains(song));
            var album = Albums.FirstOrDefault(p => p.Songs.Contains(song));
            var playlists = Playlists.Where(p => p.Songs.Count(pp => pp.SongId == song.Id) > 0).ToList();

            foreach (var playlist in playlists)
            {
                var songs = playlist.Songs.Where(p => p.SongId == song.Id).ToList();
                foreach (var playlistSong in songs)
                {
                    await DeleteFromPlaylistAsync(playlist, playlistSong);
                }

                if (playlist.Songs.Count == 0)
                {
                    await DeletePlaylistAsync(playlist);
                }
            }

            if (album != null)
            {
                album.Songs.Remove(song);
                if (album.Songs.Count == 0)
                {
                    await _sqlService.DeleteItemAsync(album);
                    Albums.Remove(album);
                }
            }

            if (artist != null)
            {
                artist.Songs.Remove(song);
                if (artist.Songs.Count == 0)
                {
                    await _sqlService.DeleteItemAsync(artist);
                    Artists.Remove(artist);
                }
            }

            //good, now lets delete it from the db
            await _sqlService.DeleteItemAsync(song);

            Songs.Remove(song);
        }

        /// <summary>
        ///     Deleting unused files.
        ///     Artworks, since deleting them when an album is delete can cause problems.
        /// </summary>
        private async void CleanupFiles()
        {
            var artworkFolder = await StorageHelper.GetFolderAsync("artworks");

            if (artworkFolder == null) return;

            var artworks = await artworkFolder.GetFilesAsync();

            foreach (var file in from file in artworks
                let id = int.Parse(file.Name.Replace(".jpg", ""))
                where Albums.Count(p => p.Id == id) == 0
                select file)
            {
                try
                {
                    await file.DeleteAsync();
                }
                catch
                {
                }
            }
        }

        private async Task<BitmapImage> GetArtworkAsync(long id)
        {
            var artworkPath = string.Format(CollectionConstant.ArtworkPath, id);

            var exists = await StorageHelper.FileExistsAsync(artworkPath);

            return exists
                ? new BitmapImage(new Uri(CollectionConstant.LocalStorageAppPath + artworkPath))
                : CollectionConstant.MissingArtworkImage;
        }

        #region Playback Queue

        public async Task ClearQueueAsync()
        {
            if (PlaybackQueue.Count == 0) return;

            await _sqlService.DeleteTableAsync<QueueSong>();
            _lookupMap.Clear();
            PlaybackQueue.Clear();
        }

        public async Task AddToQueueAsync(Song song)
        {
            var tail = PlaybackQueue.LastOrDefault();

            //Create the new queue entry
            var newQueue = new QueueSong
            {
                SongId = song.Id,
                NextId = 0,
                PrevId = tail == null ? 0 : tail.Id,
                Song = song
            };

            //Add it to the database
            await _sqlService.InsertAsync(newQueue);

            if (tail != null)
            {
                //Update the next id of the previous tail
                tail.NextId = newQueue.Id;
                await _sqlService.UpdateItemAsync(tail);
            }

            //Add the new queue entry to the collection and map
            PlaybackQueue.Add(newQueue);
            _lookupMap.Add(newQueue.Id, newQueue);
        }

        public Task MoveQueueFromToAsync(int oldIndex, int newIndex)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteFromQueueAsync(Song songToRemove)
        {
            QueueSong previousModel;

            var queueSongToRemove = PlaybackQueue.FirstOrDefault(p => p.SongId == songToRemove.Id);

            if (queueSongToRemove == null)
                return;

            if (_lookupMap.TryGetValue(queueSongToRemove.PrevId, out previousModel))
            {
                previousModel.NextId = queueSongToRemove.NextId;
                await _sqlService.UpdateItemAsync(previousModel);
            }

            QueueSong nextModel;

            if (_lookupMap.TryGetValue(queueSongToRemove.NextId, out nextModel))
            {
                nextModel.PrevId = queueSongToRemove.PrevId;
                await _sqlService.UpdateItemAsync(nextModel);
            }

            PlaybackQueue.Remove(queueSongToRemove);
            _lookupMap.Remove(queueSongToRemove.Id);

            //Delete from database
            await _sqlService.DeleteItemAsync(queueSongToRemove);
        }

        private async void LoadQueue()
        {
            var queue = _sqlService.SelectAll<QueueSong>();
            QueueSong head = null;

            foreach (var queueSong in queue)
            {
                queueSong.Song = Songs.FirstOrDefault(p => p.Id == queueSong.SongId);

                _lookupMap.Add(queueSong.Id, queueSong);
                if (queueSong.PrevId == 0)
                    head = queueSong;
            }

            if (head == null)
                return;

            for (var i = 0; i < queue.Count; i++)
            {
                if (_dispatcher != null)
                    await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => PlaybackQueue.Add(head));
                else
                    PlaybackQueue.Add(head);

                if (head.NextId != 0)
                    head = _lookupMap[head.NextId];
            }
        }

        #endregion

        #region Playlist

        public async Task<Playlist> CreatePlaylistAsync(string name)
        {
            if (Playlists.Count(p => p.Name == name) > 0)
                throw new ArgumentException(name);

            var playlist = new Playlist {Name = name};
            await _sqlService.InsertAsync(playlist);

            Playlists.Insert(0, playlist);

            return playlist;
        }

        public async Task DeletePlaylistAsync(Playlist playlist)
        {
            await _sqlService.DeleteItemAsync(playlist);
            await _sqlService.DeleteWhereAsync<PlaylistSong>("PlaylistId", playlist.Id.ToString());
            Playlists.Remove(playlist);
        }

        public async Task AddToPlaylistAsync(Playlist playlist, Song song)
        {
            var tail = playlist.Songs.LastOrDefault();

            //Create the new queue entry
            var newSong = new PlaylistSong
            {
                SongId = song.Id,
                NextId = 0,
                PrevId = tail == null ? 0 : tail.Id,
                Song = song,
                PlaylistId = playlist.Id
            };

            //Add it to the database
            await _sqlService.InsertAsync(newSong);

            if (tail != null)
            {
                //Update the next id of the previous tail
                tail.NextId = newSong.Id;
                await _sqlService.UpdateItemAsync(tail);
            }

            //Add the new queue entry to the collection and map
            playlist.Songs.Add(newSong);
            playlist.LookupMap.Add(newSong.Id, newSong);
        }

        public async Task MovePlaylistFromToAsync(Playlist playlist, int oldIndex, int newIndex)
        {
            var song = playlist.Songs[newIndex];
            var originalSong = newIndex < oldIndex 
                ? playlist.Songs[newIndex + 1] 
                : playlist.Songs[newIndex - 1];

            #region Update next and prev ids

            var songPrevId = song.PrevId;
            var songNextId = song.NextId;

            if (newIndex < oldIndex)
            {
                song.PrevId = originalSong.PrevId;
                song.NextId = originalSong.Id;
                originalSong.PrevId = song.Id;

                if (song.PrevId != 0)
                {
                    var prevSong = playlist.LookupMap[song.PrevId];
                    prevSong.NextId = song.Id;
                    await _sqlService.UpdateItemAsync(prevSong);
                }
            }
            else
            {
                song.NextId = originalSong.NextId;
                song.PrevId = originalSong.Id;
                originalSong.NextId = song.Id;

                if (song.NextId != 0)
                {
                    var nextSong = playlist.LookupMap[song.NextId];
                    nextSong.PrevId = song.Id;
                    await _sqlService.UpdateItemAsync(nextSong);
                }
            }

            #endregion

            #region update surrounding songs

            if (songPrevId != 0)
            {
                var prevSong = playlist.LookupMap[songPrevId];
                prevSong.NextId = songNextId;
                await _sqlService.UpdateItemAsync(prevSong);
            }

            if (songNextId != 0)
            {
                var nextSong = playlist.LookupMap[songNextId];
                nextSong.PrevId = songPrevId;
                await _sqlService.UpdateItemAsync(nextSong);
            }

            #endregion

            await _sqlService.UpdateItemAsync(song);
            await _sqlService.UpdateItemAsync(originalSong);
        }

        public async Task DeleteFromPlaylistAsync(Playlist playlist, PlaylistSong songToRemove)
        {
            #region update surounding entries

            if (songToRemove.NextId != 0)
            {
                var nextSong = playlist.LookupMap[songToRemove.NextId];
                nextSong.PrevId = songToRemove.PrevId;
                await _sqlService.UpdateItemAsync(nextSong);
            }

            if (songToRemove.PrevId != 0)
            {
                var prevSong = playlist.LookupMap[songToRemove.PrevId];
                prevSong.NextId = songToRemove.NextId;
                await _sqlService.UpdateItemAsync(prevSong);
            }

            #endregion

            await _sqlService.DeleteItemAsync(songToRemove);
            playlist.Songs.Remove(songToRemove);
        }

        private async void LoadPlaylists()
        {
            var playlists = _sqlService.SelectAll<Playlist>().OrderByDescending(p => p.Id);
            var playlistSongs = _sqlService.SelectAll<PlaylistSong>();

            foreach (var playlist in playlists)
            {
                var songs = playlistSongs.Where(p => p.PlaylistId == playlist.Id).ToList();


                PlaylistSong head = null;

                foreach (var playlistSong in songs)
                {
                    playlistSong.Song = Songs.FirstOrDefault(p => p.Id == playlistSong.SongId);

                    playlist.LookupMap.Add(playlistSong.Id, playlistSong);
                    if (playlistSong.PrevId == 0)
                        head = playlistSong;
                }

                #region order songs

                if (head != null)
                {
                    for (var i = 0; i < songs.Count; i++)
                    {
                        playlist.Songs.Add(head);

                        if (head.NextId != 0)
                            head = playlist.LookupMap[head.NextId];
                    }
                }

                #endregion

                if (_dispatcher != null)
                    await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Playlists.Add(playlist));
                else
                    Playlists.Add(playlist);
            }
        }

        #endregion
    }
}