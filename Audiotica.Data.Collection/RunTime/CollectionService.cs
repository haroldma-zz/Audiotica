using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Audiotica.Core.Common;
using Audiotica.Core.Utils;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Data.Collection.Model;

using PCLStorage;

using SQLite;

using TagLib;

namespace Audiotica.Data.Collection.RunTime
{
    public class CollectionService : INotifyPropertyChanged, ICollectionService
    {
        private readonly IAppSettingsHelper _appSettingsHelper;

        private readonly string _artistArtworkFilePath;

        private readonly string _artworkFilePath;

        private readonly ISqlService _bgSqlService;

        private readonly IBitmapFactory _bitmapFactory;

        private readonly IDispatcherHelper _dispatcher;

        private readonly string _localFilePrefix;

        private readonly ConcurrentDictionary<long, QueueSong> _lookupMap = new ConcurrentDictionary<long, QueueSong>();

        private readonly IBitmapImage _missingArtwork;

        private readonly ISqlService _sqlService;

        public CollectionService(
            ISqlService sqlService, 
            ISqlService bgSqlService, 
            IDispatcherHelper dispatcher, 
            IAppSettingsHelper appSettingsHelper, 
            IBitmapFactory bitmapFactory, 
            IBitmapImage missingArtwork, 
            string localFilePrefix, 
            string artworkFilePath, 
            string artistArtworkFilePath)
        {
            this._bgSqlService = bgSqlService;
            this._sqlService = sqlService;
            this._dispatcher = dispatcher;
            this._appSettingsHelper = appSettingsHelper;
            this._bitmapFactory = bitmapFactory;
            this._missingArtwork = missingArtwork;
            this._localFilePrefix = localFilePrefix;
            this._artworkFilePath = artworkFilePath;
            this._artistArtworkFilePath = artistArtworkFilePath;
            this.Songs = new OptimizedObservableCollection<Song>();
            this.Artists = new OptimizedObservableCollection<Artist>();
            this.Albums = new OptimizedObservableCollection<Album>();
            this.Playlists = new OptimizedObservableCollection<Playlist>();
            this.PlaybackQueue = new OptimizedObservableCollection<QueueSong>();
            this.ShufflePlaybackQueue = new OptimizedObservableCollection<QueueSong>();
        }

        private bool IsShuffle
        {
            get
            {
                return this._appSettingsHelper.Read<bool>("Shuffle");
            }
        }

        public bool IsLibraryLoaded { get; private set; }

        public event EventHandler LibraryLoaded;

        public OptimizedObservableCollection<Song> Songs { get; set; }

        public OptimizedObservableCollection<Song> TempSongs { get; set; }

        public OptimizedObservableCollection<Album> Albums { get; set; }

        public OptimizedObservableCollection<Album> TempAlbums { get; set; }

        public OptimizedObservableCollection<Artist> Artists { get; set; }

        public OptimizedObservableCollection<Artist> TempArtists { get; set; }

        public OptimizedObservableCollection<RadioStation> Stations { get; set; }

        public OptimizedObservableCollection<Playlist> Playlists { get; set; }

        public OptimizedObservableCollection<QueueSong> CurrentPlaybackQueue
        {
            get
            {
                return this.IsShuffle ? this.ShufflePlaybackQueue : this.PlaybackQueue;
            }
        }

        public OptimizedObservableCollection<QueueSong> PlaybackQueue { get; private set; }

        public OptimizedObservableCollection<QueueSong> ShufflePlaybackQueue { get; private set; }

        public int ScaledImageSize { get; set; }

        public void LoadLibrary(bool loadEssentials = false)
        {
            if (this.IsLibraryLoaded)
            {
                return;
            }

            /*
             * Sqlite makes a transaction to create a shared lock
             * Wrapping it in one single transactions assures it is only lock and release once
             */
            this._sqlService.BeginTransaction();

            var songs = this._sqlService.SelectAll<Song>().OrderByDescending(p => p.Id).ToList();
            var artists = this._sqlService.SelectAll<Artist>().OrderByDescending(p => p.Id).ToList();
            var albums = new List<Album>();
            if (!loadEssentials)
            {
                albums = this._sqlService.SelectAll<Album>().OrderByDescending(p => p.Id).ToList();
            }

            // Let go of the lock
            this._sqlService.Commit();

            var isForeground = this._dispatcher != null;

            foreach (var song in songs)
            {
                song.Artist = artists.FirstOrDefault(p => p.Id == song.ArtistId);
                song.Album = albums.FirstOrDefault(p => p.Id == song.AlbumId);
            }

            if (isForeground)
            {
                this._dispatcher.RunAsync(() => this.Songs.AddRange(songs)).Wait();
            }
            else
            {
                this.Songs.AddRange(songs);
            }

            foreach (var album in albums)
            {
                album.Songs.AddRange(songs.Where(p => p.AlbumId == album.Id).OrderBy(p => p.TrackNumber));
                album.PrimaryArtist = artists.FirstOrDefault(p => p.Id == album.PrimaryArtistId);

                if (isForeground)
                {
                    this._dispatcher.RunAsync(
                        () =>
                            {
                                var artworkPath = string.Format(this._artworkFilePath, album.Id);
                                if (album.HasArtwork)
                                {
                                    var path = this._localFilePrefix + artworkPath;

                                    album.Artwork = this._bitmapFactory.CreateImage(new Uri(path));

                                    if (this.ScaledImageSize != 0)
                                    {
                                        album.Artwork.SetDecodedPixel(this.ScaledImageSize);

                                        album.MediumArtwork = this._bitmapFactory.CreateImage(new Uri(path));
                                        album.MediumArtwork.SetDecodedPixel(this.ScaledImageSize / 2);

                                        album.SmallArtwork = this._bitmapFactory.CreateImage(new Uri(path));
                                        album.SmallArtwork.SetDecodedPixel(50);
                                    }
                                }
                                else
                                {
                                    album.Artwork = this._missingArtwork;
                                    album.MediumArtwork = this._missingArtwork;
                                    album.SmallArtwork = this._missingArtwork;
                                }
                            }).Wait();
                }
            }

            if (isForeground)
            {
                this._dispatcher.RunAsync(() => this.Albums.AddRange(albums)).Wait();
            }
            else
            {
                this.Albums.AddRange(albums);
            }

            foreach (var artist in artists)
            {
                artist.Songs.AddRange(songs.Where(p => p.ArtistId == artist.Id).OrderBy(p => p.Name));
                artist.Albums.AddRange(albums.Where(p => p.PrimaryArtistId == artist.Id).OrderBy(p => p.Name));

                var songsAlbums = artist.Songs.Select(p => p.Album);
                artist.Albums.AddRange(songsAlbums.Where(p => !artist.Albums.Contains(p)));
                if (isForeground)
                {
                    this._dispatcher.RunAsync(
                        () =>
                            {
                                var artworkPath = string.Format(this._artistArtworkFilePath, artist.Id);
                                artist.Artwork = artist.HasArtwork
                                                     ? this._bitmapFactory.CreateImage(
                                                         new Uri(this._localFilePrefix + artworkPath))
                                                     : null;

                                if (this.ScaledImageSize != 0 && artist.Artwork != null)
                                {
                                    artist.Artwork.SetDecodedPixel(this.ScaledImageSize);
                                }
                            }).Wait();
                }
            }

            if (isForeground)
            {
                this._dispatcher.RunAsync(() => this.Artists.AddRange(artists)).Wait();
            }
            else
            {
                this.Artists.AddRange(artists);
            }

            this.IsLibraryLoaded = true;

            this.LoadQueue(songs);

            if (!loadEssentials)
            {
                this.LoadPlaylists();
            }

            if (!isForeground)
            {
                return;
            }

            var corruptSongs =
                this.Songs.ToList()
                    .Where(p => string.IsNullOrEmpty(p.Name) || p.Album == null || p.Artist == null)
                    .ToList();
            foreach (var corruptSong in corruptSongs)
            {
                this.DeleteSongAsync(corruptSong).Wait();
            }

            this._dispatcher.RunAsync(
                () =>
                    {
                        if (this.LibraryLoaded != null)
                        {
                            this.LibraryLoaded(this, null);
                        }
                    }).Wait();

            try
            {
                this.CleanupFiles(albums, artists);
            }
            catch
            {
                // ignored
            }
        }

        public Task LoadLibraryAsync(bool loadEssentials = false)
        {
            // just return non async as a task
            return Task.Factory.StartNew(() => this.LoadLibrary(loadEssentials));
        }

        public bool SongAlreadyExists(string localSongPath)
        {
            return
                this.Songs.FirstOrDefault(
                    p =>
                    (p.SongState == SongState.Local || p.SongState == SongState.Downloaded)
                    && localSongPath == p.AudioUrl) != null;
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
            this.OnPropertyChanged("CurrentPlaybackQueue");
        }

        public bool SongAlreadyExists(string providerId, string name, string album, string artist)
        {
            return
                this.Songs.FirstOrDefault(
                    p =>
                    p.ProviderId == providerId
                    || (p.Name == name && p.Album.Name == album && (p.ArtistName == artist || p.Artist.Name == artist)))
                != null;
        }

        public async Task DeleteSongAsync(Song song, bool temp = false)
        {
            var queueSong = this.PlaybackQueue.FirstOrDefault(p => p.SongId == song.Id);
            if (queueSong != null)
            {
                await this.DeleteFromQueueAsync(queueSong);
            }

            // remove it from artist, albums and playlists songs
            var playlists = this.Playlists.Where(p => p.Songs.Count(pp => pp.SongId == song.Id) > 0).ToList();

            foreach (var playlist in playlists)
            {
                var songs = playlist.Songs.Where(p => p.SongId == song.Id).ToList();
                foreach (var playlistSong in songs)
                {
                    await this.DeleteFromPlaylistAsync(playlist, playlistSong);
                }

                if (playlist.Songs.Count == 0)
                {
                    await this.DeletePlaylistAsync(playlist);
                }
            }

            if (song.Album != null)
            {
                song.Album.Songs.Remove(song);
                if (song.Album.Songs.Count == 0)
                {
                    await this._sqlService.DeleteItemAsync(song.Album);
                    await this._dispatcher.RunAsync(
                        () =>
                            {
                                this.Albums.Remove(song.Album);
                                song.Artist.Albums.Remove(song.Album);

                                // var tileId = "album." + song.AlbumId;
                                // if (SecondaryTile.Exists(tileId))
                                // {
                                // var secondaryTile = new SecondaryTile(tileId);
                                // secondaryTile.RequestDeleteAsync();
                                // }
                            });
                }
            }

            if (song.Artist != null)
            {
                song.Artist.Songs.Remove(song);
                if (song.Artist.Songs.Count == 0)
                {
                    await this._sqlService.DeleteItemAsync(song.Artist);
                    await this._dispatcher.RunAsync(
                        () =>
                            {
                                this.Artists.Remove(song.Artist);

                                // var tileId = "artist." + song.ArtistId;
                                // if (SecondaryTile.Exists(tileId))
                                // {
                                // var secondaryTile = new SecondaryTile(tileId);
                                // secondaryTile.RequestDeleteAsync();
                                // }
                            });
                }
            }

            // good, now lets delete it from the db
            await this._sqlService.DeleteItemAsync(song);

            await this._dispatcher.RunAsync(() => this.Songs.Remove(song));
        }

        public async Task<List<HistoryEntry>> FetchHistoryAsync()
        {
            var list = await Task.FromResult(this._bgSqlService.SelectAll<HistoryEntry>().ToList());
            foreach (var historyEntry in list)
            {
                historyEntry.Song = this.Songs.FirstOrDefault(p => p.Id == historyEntry.SongId);
            }

            return list;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public async Task AddSongAsync(Song song, Tag tags = null, bool temp = false)
        {
            var primaryArtist = (song.Album == null ? song.Artist : song.Album.PrimaryArtist)
                                ?? new Artist { Name = "Unknown Artist", ProviderId = "autc.unknown" };
            var artist =
                this.Artists.FirstOrDefault(
                    entry =>
                    entry.ProviderId == primaryArtist.ProviderId
                    || string.Equals(entry.Name, primaryArtist.Name, StringComparison.CurrentCultureIgnoreCase));
            if (artist == null)
            {
                await this._sqlService.InsertAsync(primaryArtist);
                await this._dispatcher.RunAsync(() => this.Artists.Insert(0, primaryArtist));

                song.Artist = primaryArtist;
                song.ArtistId = primaryArtist.Id;

                if (song.Album != null)
                {
                    song.Album.PrimaryArtistId = song.Artist.Id;
                    song.Album.PrimaryArtist = song.Artist;
                }
            }
            else
            {
                song.Artist = artist;

                if (song.Album != null)
                {
                    song.Album.PrimaryArtistId = artist.Id;
                    song.Album.PrimaryArtist = artist;
                }
            }

            song.ArtistId = song.Artist.Id;

            if (song.Album == null)
            {
                song.Album = new Album
                {
                    PrimaryArtistId = song.ArtistId, 
                    Name = song.Name, 
                    PrimaryArtist = song.Artist, 
                    ProviderId = "autc.single." + song.ProviderId
                };
            }

            var album = this.Albums.FirstOrDefault(p => p.ProviderId == song.Album.ProviderId);

            if (album != null)
            {
                song.Album = album;
            }
            else
            {
                await this._sqlService.InsertAsync(song.Album);
                await this._dispatcher.RunAsync(() =>
                {
                    this.Albums.Insert(0, song.Album);
                    song.Album.Artwork = this._missingArtwork;
                    song.Album.MediumArtwork = this._missingArtwork;
                    song.Album.SmallArtwork = this._missingArtwork;
                });

                if (tags != null && tags.Pictures != null && tags.Pictures.Length > 0)
                {
                    var albumFilePath = string.Format(this._artworkFilePath, song.Album.Id);
                    Stream artwork = null;

                    var image = tags.Pictures.FirstOrDefault();
                    if (image != null)
                    {
                        artwork = new MemoryStream(image.Data.Data);
                    }

                    if (artwork != null)
                    {
                        using (artwork)
                        {
                            try
                            {
                                var file =
                                    await
                                        StorageHelper.CreateFileAsync(
                                            albumFilePath,
                                            option: CreationCollisionOption.ReplaceExisting);

                                using (var fileStream = await file.OpenAsync(FileAccess.ReadAndWrite))
                                {
                                    var bytes = tags.Pictures[0].Data.Data;
                                    await fileStream.WriteAsync(bytes, 0, bytes.Length);
                                    song.Album.HasArtwork = true;
                                    await this._sqlService.UpdateItemAsync(song.Album);
                                }
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }

                    // set it
                    if (song.Album.HasArtwork)
                    {
                        await this._dispatcher.RunAsync(
                            () =>
                            {
                                var path = this._localFilePrefix + albumFilePath;

                                song.Album.Artwork = this._bitmapFactory.CreateImage(new Uri(path));

                                if (this.ScaledImageSize == 0)
                                {
                                    return;
                                }

                                song.Album.Artwork.SetDecodedPixel(this.ScaledImageSize);

                                song.Album.MediumArtwork = this._bitmapFactory.CreateImage(new Uri(path));
                                song.Album.MediumArtwork.SetDecodedPixel(this.ScaledImageSize / 2);

                                song.Album.SmallArtwork = this._bitmapFactory.CreateImage(new Uri(path));
                                song.Album.SmallArtwork.SetDecodedPixel(50);
                            });
                    }
                }
            }

            song.AlbumId = song.Album.Id;

            if (string.IsNullOrEmpty(song.ArtistName)) song.ArtistName = song.Artist.Name;

            await this._sqlService.InsertAsync(song);

            await this._dispatcher.RunAsync(
                () =>
                    {
                        var orderedAlbumSong = song.Album.Songs.ToList();
                        orderedAlbumSong.Add(song);
                        orderedAlbumSong = orderedAlbumSong.OrderBy(p => p.TrackNumber).ToList();

                        var index = orderedAlbumSong.IndexOf(song);
                        song.Album.Songs.Insert(index, song);


                        var orderedArtistSong = song.Artist.Songs.ToList();
                        orderedArtistSong.Add(song);
                        orderedArtistSong = orderedArtistSong.OrderBy(p => p.Name).ToList();

                        index = orderedArtistSong.IndexOf(song);
                        song.Artist.Songs.Insert(index, song);


                        #region Order artist album

                        if (!song.Artist.Albums.Contains(song.Album))
                        {
                            var orderedArtistAlbum = song.Artist.Albums.ToList();
                            orderedArtistAlbum.Add(song.Album);
                            orderedArtistAlbum = orderedArtistAlbum.OrderBy(p => p.Name).ToList();

                            index = orderedArtistAlbum.IndexOf(song.Album);
                            song.Artist.Albums.Insert(index, song.Album);
                        }

                        #endregion

                        this.Songs.Insert(0, song);
                    });
        }

        private async void CleanupFiles(IEnumerable<Album> albums, IEnumerable<Artist> artists)
        {
            var artworkFolder = await StorageHelper.GetFolderAsync("artworks");

            if (artworkFolder == null)
            {
                return;
            }

            var artworks = await artworkFolder.GetFilesAsync();

            foreach (var file in from file in artworks
                                 let id = file.Name.Replace(".jpg", string.Empty)
                                 where
                                     albums.FirstOrDefault(p => p.Id.ToString() == id) == null
                                     && artists.FirstOrDefault(p => p.ProviderId == id) == null
                                 select file)
            {
                try
                {
                    await file.DeleteAsync();
                    Debug.WriteLine("Deleted file: {0}", file.Name);
                }
                catch
                {
                    // ignored
                }
            }
        }

        #region Playback Queue

        public async Task ClearQueueAsync()
        {
            if (this.PlaybackQueue.Count == 0)
            {
                return;
            }

            await this._bgSqlService.DeleteTableAsync<QueueSong>();

            this._lookupMap.Clear();
            await this._dispatcher.RunAsync(
                () =>
                    {
                        this.PlaybackQueue.Clear();
                        this.ShufflePlaybackQueue.Clear();
                    });
        }

        public async Task ShuffleCurrentQueueAsync()
        {
            var unshuffle = this.PlaybackQueue.ToList().Shuffle();

            if (unshuffle.Count >= 5)
            {
                await this._dispatcher.RunAsync(() => this.ShufflePlaybackQueue.SwitchTo(unshuffle));

                for (var i = 0; i < unshuffle.Count; i++)
                {
                    var queueSong = unshuffle[i];

                    queueSong.ShufflePrevId = i == 0 ? 0 : unshuffle[i - 1].Id;

                    if (i + 1 < unshuffle.Count)
                    {
                        queueSong.ShuffleNextId = unshuffle[i + 1].Id;
                    }

                    await this._bgSqlService.UpdateItemAsync(queueSong);
                }
            }
        }

        public async Task<QueueSong> AddToQueueAsync(Song song, QueueSong position = null, bool shuffleInsert = true, bool temp = false)
        {
            if (song == null)
            {
                return null;
            }

            var rnd = new Random(DateTime.Now.Millisecond);
            QueueSong prev = null;
            QueueSong shufflePrev = null;
            QueueSong next = null;
            QueueSong shuffleNext = null;
            var shuffleIndex = -1;
            var normalIndex = -1;

            if (position != null)
            {
                shuffleIndex = this.ShufflePlaybackQueue.IndexOf(position) + 1;
                normalIndex = this.PlaybackQueue.IndexOf(position) + 1;
            }

            var insert = normalIndex > -1 && normalIndex < this.PlaybackQueue.Count;
            var insertShuffle = shuffleIndex > -1 && shuffleInsert;
            var shuffleLastAdd = shuffleIndex == this.ShufflePlaybackQueue.Count;

            if (insert)
            {
                next = this.PlaybackQueue.ElementAtOrDefault(normalIndex);
                if (next != null)
                {
                    this._lookupMap.TryGetValue(next.PrevId, out prev);
                }
            }
            else
            {
                prev = this.PlaybackQueue.LastOrDefault();
            }

            if (insertShuffle)
            {
                if (shuffleLastAdd)
                {
                    shufflePrev = this.ShufflePlaybackQueue.ElementAtOrDefault(this.ShufflePlaybackQueue.Count - 1);
                }
                else
                {
                    shuffleNext = this.ShufflePlaybackQueue.ElementAtOrDefault(shuffleIndex);
                    if (shuffleNext != null)
                    {
                        this._lookupMap.TryGetValue(shuffleNext.ShufflePrevId, out shufflePrev);
                    }
                }
            }
            else
            {
                if (this.ShufflePlaybackQueue.Count > 1)
                {
                    shuffleIndex = rnd.Next(1, this.ShufflePlaybackQueue.Count - 1);
                    shuffleNext = this.ShufflePlaybackQueue.ElementAt(shuffleIndex);

                    this._lookupMap.TryGetValue(shuffleNext.ShufflePrevId, out shufflePrev);
                }
                else
                {
                    shuffleLastAdd = true;
                    shufflePrev = prev;
                }
            }

            // Create the new queue entry
            var newQueue = new QueueSong
            {
                SongId = song.Id, 
                NextId = next == null ? 0 : next.Id, 
                PrevId = prev == null ? 0 : prev.Id, 
                ShuffleNextId = shuffleNext == null ? 0 : shuffleNext.Id, 
                ShufflePrevId = shufflePrev == null ? 0 : shufflePrev.Id, 
                Song = song
            };

            // Add it to the database
            await this._bgSqlService.InsertAsync(newQueue).ConfigureAwait(false);

            if (next != null)
            {
                // Update the prev id of the queue that was replaced
                next.PrevId = newQueue.Id;
                await this._bgSqlService.UpdateItemAsync(next).ConfigureAwait(false);
            }

            if (prev != null)
            {
                // Update the next id of the previous tail
                prev.NextId = newQueue.Id;
                await this._bgSqlService.UpdateItemAsync(prev).ConfigureAwait(false);
            }

            if (shuffleNext != null)
            {
                shuffleNext.ShufflePrevId = newQueue.Id;
                await this._bgSqlService.UpdateItemAsync(shuffleNext).ConfigureAwait(false);
            }

            if (shufflePrev != null)
            {
                shufflePrev.ShuffleNextId = newQueue.Id;
                await this._bgSqlService.UpdateItemAsync(shufflePrev).ConfigureAwait(false);
            }

            // Add the new queue entry to the collection and map
            await this._dispatcher.RunAsync(
                () =>
                    {
                        if (insert)
                        {
                            try
                            {
                                this.PlaybackQueue.Insert(normalIndex, newQueue);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                this.PlaybackQueue.Add(newQueue);
                            }
                        }
                        else
                        {
                            this.PlaybackQueue.Add(newQueue);
                        }

                        if (shuffleLastAdd || !shuffleInsert)
                        {
                            this.ShufflePlaybackQueue.Add(newQueue);
                        }
                        else
                        {
                            try
                            {
                                this.ShufflePlaybackQueue.Insert(shuffleIndex, newQueue);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                this.ShufflePlaybackQueue.Add(newQueue);
                            }
                        }
                    }).ConfigureAwait(false);

            this._lookupMap.TryAdd(newQueue.Id, newQueue);

            return newQueue;
        }

        public Task MoveQueueFromToAsync(int oldIndex, int newIndex)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteFromQueueAsync(QueueSong songToRemove)
        {
            QueueSong previousModel;

            if (songToRemove == null)
            {
                return;
            }

            if (this._lookupMap.TryGetValue(songToRemove.PrevId, out previousModel))
            {
                previousModel.NextId = songToRemove.NextId;
                await this._bgSqlService.UpdateItemAsync(previousModel);
            }

            if (this._lookupMap.TryGetValue(songToRemove.ShufflePrevId, out previousModel))
            {
                previousModel.ShuffleNextId = songToRemove.ShuffleNextId;
                await this._bgSqlService.UpdateItemAsync(previousModel);
            }

            QueueSong nextModel;

            if (this._lookupMap.TryGetValue(songToRemove.NextId, out nextModel))
            {
                nextModel.PrevId = songToRemove.PrevId;
                await this._bgSqlService.UpdateItemAsync(nextModel);
            }

            if (this._lookupMap.TryGetValue(songToRemove.ShuffleNextId, out nextModel))
            {
                nextModel.ShufflePrevId = songToRemove.ShufflePrevId;
                await this._bgSqlService.UpdateItemAsync(nextModel);
            }

            await this._dispatcher.RunAsync(
                () =>
                    {
                        this.PlaybackQueue.Remove(songToRemove);
                        this.CurrentPlaybackQueue.Remove(songToRemove);
                    });
            this._lookupMap.TryRemove(songToRemove.Id, out songToRemove);

            // Delete from database
            await this._bgSqlService.DeleteItemAsync(songToRemove);
        }

        private void LoadQueue(List<Song> songs)
        {
            var queue = this._bgSqlService.SelectAll<QueueSong>();
            QueueSong head = null;
            QueueSong shuffleHead = null;

            foreach (var queueSong in queue)
            {
                queueSong.Song = songs.FirstOrDefault(p => p.Id == queueSong.SongId);

                this._lookupMap.TryAdd(queueSong.Id, queueSong);

                if (queueSong.ShufflePrevId == 0)
                {
                    shuffleHead = queueSong;
                }

                if (queueSong.PrevId == 0)
                {
                    head = queueSong;
                }
            }

            if (head != null)
            {
                for (var i = 0; i < queue.Count; i++)
                {
                    if (this._dispatcher != null)
                    {
                        this._dispatcher.RunAsync(() => this.PlaybackQueue.Add(head)).Wait();
                    }
                    else
                    {
                        this.PlaybackQueue.Add(head);
                    }

                    if (head.NextId != 0 && this._lookupMap.ContainsKey(head.NextId))
                    {
                        head = this._lookupMap[head.NextId];
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (shuffleHead != null)
            {
                for (var i = 0; i < queue.Count; i++)
                {
                    if (this._dispatcher != null)
                    {
                        this._dispatcher.RunAsync(() => this.ShufflePlaybackQueue.Add(shuffleHead)).Wait();
                    }
                    else
                    {
                        this.ShufflePlaybackQueue.Add(shuffleHead);
                    }

                    if (shuffleHead.ShuffleNextId != 0 && this._lookupMap.ContainsKey(shuffleHead.ShuffleNextId))
                    {
                        shuffleHead = this._lookupMap[shuffleHead.ShuffleNextId];
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        #endregion

        #region Playlist

        public async Task<Playlist> CreatePlaylistAsync(string name)
        {
            if (this.Playlists.Count(p => p.Name == name) > 0)
            {
                throw new ArgumentException(name);
            }

            var playlist = new Playlist { Name = name };
            await this._sqlService.InsertAsync(playlist);

            this.Playlists.Insert(0, playlist);

            return playlist;
        }

        public async Task DeletePlaylistAsync(Playlist playlist)
        {
            await this._sqlService.DeleteItemAsync(playlist);
            await this._sqlService.DeleteWhereAsync(playlist);
            this.Playlists.Remove(playlist);
        }

        public async Task AddToPlaylistAsync(Playlist playlist, Song song)
        {
            var tail = playlist.Songs.LastOrDefault();

            // Create the new queue entry
            var newSong = new PlaylistSong
            {
                SongId = song.Id, 
                NextId = 0, 
                PrevId = tail == null ? 0 : tail.Id, 
                Song = song, 
                PlaylistId = playlist.Id
            };

            // Add it to the database
            await this._sqlService.InsertAsync(newSong);

            if (tail != null)
            {
                // Update the next id of the previous tail
                tail.NextId = newSong.Id;
                await this._sqlService.UpdateItemAsync(tail);
            }

            // Add the new queue entry to the collection and map
            playlist.Songs.Add(newSong);
            playlist.LookupMap.TryAdd(newSong.Id, newSong);
        }

        public async Task MovePlaylistFromToAsync(Playlist playlist, int oldIndex, int newIndex)
        {
            var song = playlist.Songs.ElementAtOrDefault(newIndex);
            if (song == null)
            {
                return;
            }

            var originalSong = newIndex < oldIndex
                                   ? playlist.Songs.ElementAtOrDefault(newIndex + 1)
                                   : playlist.Songs.ElementAtOrDefault(newIndex - 1);
            if (originalSong == null)
            {
                return;
            }

            var songPrevId = song.PrevId;
            var songNextId = song.NextId;

            if (newIndex < oldIndex)
            {
                song.PrevId = originalSong.PrevId;
                song.NextId = originalSong.Id;
                originalSong.PrevId = song.Id;

                if (song.PrevId != 0)
                {
                    PlaylistSong prevSong;
                    if (playlist.LookupMap.TryGetValue(song.PrevId, out prevSong))
                    {
                        prevSong.NextId = song.Id;
                        await this._sqlService.UpdateItemAsync(prevSong);
                    }
                }
            }
            else
            {
                song.NextId = originalSong.NextId;
                song.PrevId = originalSong.Id;
                originalSong.NextId = song.Id;

                if (song.NextId != 0)
                {
                    PlaylistSong nextSong;
                    if (playlist.LookupMap.TryGetValue(song.NextId, out nextSong))
                    {
                        nextSong.PrevId = song.Id;
                        await this._sqlService.UpdateItemAsync(nextSong);
                    }
                }
            }

            if (songPrevId != 0)
            {
                PlaylistSong prevSong;
                if (playlist.LookupMap.TryGetValue(songPrevId, out prevSong))
                {
                    prevSong.NextId = songNextId;
                    await this._sqlService.UpdateItemAsync(prevSong);
                }
            }

            if (songNextId != 0)
            {
                PlaylistSong nextSong;
                if (playlist.LookupMap.TryGetValue(songNextId, out nextSong))
                {
                    nextSong.PrevId = songPrevId;
                    await this._sqlService.UpdateItemAsync(nextSong);
                }
            }

            await this._sqlService.UpdateItemAsync(song);
            await this._sqlService.UpdateItemAsync(originalSong);
        }

        public async Task DeleteFromPlaylistAsync(Playlist playlist, PlaylistSong songToRemove)
        {
            if (songToRemove.NextId != 0)
            {
                var nextSong = playlist.LookupMap[songToRemove.NextId];
                nextSong.PrevId = songToRemove.PrevId;
                await this._sqlService.UpdateItemAsync(nextSong);
            }

            if (songToRemove.PrevId != 0)
            {
                var prevSong = playlist.LookupMap[songToRemove.PrevId];
                prevSong.NextId = songToRemove.NextId;
                await this._sqlService.UpdateItemAsync(prevSong);
            }

            await this._sqlService.DeleteItemAsync(songToRemove);
            await this._dispatcher.RunAsync(() => playlist.Songs.Remove(songToRemove));
        }

        private async void LoadPlaylists()
        {
            var playlists = this._sqlService.SelectAll<Playlist>().OrderByDescending(p => p.Id);
            var playlistSongs = this._sqlService.SelectAll<PlaylistSong>();

            foreach (var playlist in playlists)
            {
                var songs = playlistSongs.Where(p => p.PlaylistId == playlist.Id).ToList();

                PlaylistSong head = null;

                foreach (var playlistSong in songs)
                {
                    playlistSong.Song = this.Songs.FirstOrDefault(p => p.Id == playlistSong.SongId);

                    playlist.LookupMap.TryAdd(playlistSong.Id, playlistSong);
                    if (playlistSong.PrevId == 0)
                    {
                        head = playlistSong;
                    }
                }

                if (head != null)
                {
                    for (var i = 0; i < songs.Count; i++)
                    {
                        playlist.Songs.Add(head);

                        if (head.NextId != 0)
                        {
                            head = playlist.LookupMap[head.NextId];
                        }
                    }
                }

                if (this._dispatcher != null)
                {
                    await this._dispatcher.RunAsync(() => this.Playlists.Add(playlist));
                }
                else
                {
                    this.Playlists.Add(playlist);
                }
            }
        }

        #endregion
    }
}