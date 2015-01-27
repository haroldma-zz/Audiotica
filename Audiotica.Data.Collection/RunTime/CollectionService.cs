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

using TagLib;

namespace Audiotica.Data.Collection.RunTime
{
    public class CollectionService : INotifyPropertyChanged, ICollectionService
    {
        private readonly IAppSettingsHelper appSettingsHelper;

        private readonly string artistArtworkFilePath;

        private readonly string artworkFilePath;

        private readonly ISqlService bgSqlService;

        private readonly IBitmapFactory bitmapFactory;

        private readonly IDispatcherHelper dispatcher;

        private readonly string localFilePrefix;

        private readonly ConcurrentDictionary<long, QueueSong> lookupMap = new ConcurrentDictionary<long, QueueSong>();

        private readonly IBitmapImage missingArtwork;

        private readonly ISqlService sqlService;

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
            this.bgSqlService = bgSqlService;
            this.sqlService = sqlService;
            this.dispatcher = dispatcher;
            this.appSettingsHelper = appSettingsHelper;
            this.bitmapFactory = bitmapFactory;
            this.missingArtwork = missingArtwork;
            this.localFilePrefix = localFilePrefix;
            this.artworkFilePath = artworkFilePath;
            this.artistArtworkFilePath = artistArtworkFilePath;
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
                return this.appSettingsHelper.Read<bool>("Shuffle");
            }
        }

        public bool IsLibraryLoaded { get; private set; }

        public event EventHandler LibraryLoaded;

        public OptimizedObservableCollection<Song> Songs { get; set; }

        public OptimizedObservableCollection<Album> Albums { get; set; }

        public OptimizedObservableCollection<Artist> Artists { get; set; }

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
            this.sqlService.BeginTransaction();

            var songs = this.sqlService.SelectAll<Song>().OrderByDescending(p => p.Id).ToList();
            var artists = this.sqlService.SelectAll<Artist>().OrderByDescending(p => p.Id).ToList();
            var albums = new List<Album>();
            if (!loadEssentials)
            {
                albums = this.sqlService.SelectAll<Album>().OrderByDescending(p => p.Id).ToList();
            }

            // Let go of the lock
            this.sqlService.Commit();

            var isForeground = this.dispatcher != null;

            foreach (var song in songs)
            {
                song.Artist = artists.FirstOrDefault(p => p.Id == song.ArtistId);
                song.Album = albums.FirstOrDefault(p => p.Id == song.AlbumId);
            }

            if (isForeground)
            {
                this.dispatcher.RunAsync(() => this.Songs.AddRange(songs)).Wait();
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
                    this.dispatcher.RunAsync(
                        () =>
                            {
                                var artworkPath = string.Format(this.artworkFilePath, album.Id);
                                if (album.HasArtwork)
                                {
                                    var path = this.localFilePrefix + artworkPath;

                                    album.Artwork = this.bitmapFactory.CreateImage(new Uri(path));

                                    if (this.ScaledImageSize != 0)
                                    {
                                        album.Artwork.SetDecodedPixel(this.ScaledImageSize);

                                        album.MediumArtwork = this.bitmapFactory.CreateImage(new Uri(path));
                                        album.MediumArtwork.SetDecodedPixel(this.ScaledImageSize / 2);

                                        album.SmallArtwork = this.bitmapFactory.CreateImage(new Uri(path));
                                        album.SmallArtwork.SetDecodedPixel(50);
                                    }
                                }
                                else
                                {
                                    album.Artwork = this.missingArtwork;
                                    album.MediumArtwork = this.missingArtwork;
                                    album.SmallArtwork = this.missingArtwork;
                                }
                            }).Wait();
                }
            }

            if (isForeground)
            {
                this.dispatcher.RunAsync(() => this.Albums.AddRange(albums)).Wait();
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
                    this.dispatcher.RunAsync(
                        () =>
                            {
                                var artworkPath = string.Format(this.artistArtworkFilePath, artist.Id);
                                artist.Artwork = artist.HasArtwork
                                                     ? this.bitmapFactory.CreateImage(
                                                         new Uri(this.localFilePrefix + artworkPath))
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
                this.dispatcher.RunAsync(() => this.Artists.AddRange(artists)).Wait();
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

            this.dispatcher.RunAsync(
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

        public Task AddSongAsync(Song song, string artworkUrl)
        {
            return this.AddSongAsync(song, null, artworkUrl);
        }

        public Task AddSongAsync(Song song, Tag tags)
        {
            return this.AddSongAsync(song, tags, null);
        }

        public async Task DeleteSongAsync(Song song)
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
                    await this.sqlService.DeleteItemAsync(song.Album);
                    await this.dispatcher.RunAsync(
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
                    await this.sqlService.DeleteItemAsync(song.Artist);
                    await this.dispatcher.RunAsync(
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
            await this.sqlService.DeleteItemAsync(song);

            await this.dispatcher.RunAsync(() => this.Songs.Remove(song));
        }

        public async Task<List<HistoryEntry>> FetchHistoryAsync()
        {
            var list = await Task.FromResult(this.bgSqlService.SelectAll<HistoryEntry>().ToList());
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

        private async Task AddSongAsync(Song song, Tag tags, string artworkUrl)
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
                await this.sqlService.InsertAsync(primaryArtist);
                await this.dispatcher.RunAsync(() => this.Artists.Insert(0, primaryArtist));

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

            

            #region create album

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
                await this.sqlService.InsertAsync(song.Album);
                await this.dispatcher.RunAsync(() => this.Albums.Insert(0, song.Album));

                #region Download artwork

                var albumFilePath = string.Format(this.artworkFilePath, song.Album.Id);

                if (tags != null && tags.Pictures != null)
                {
                    Stream artwork = null;

                    var image = tags.Pictures.FirstOrDefault();
                    if (image != null)
                    {
                        artwork = new MemoryStream(image.Data.Data);
                    }

                    if (artwork != null)
                    {
                        song.Album.HasArtwork = await this.GetArtworkAsync(albumFilePath, artwork);
                        await this.sqlService.UpdateItemAsync(song.Album);
                        artwork.Dispose();
                    }
                }
                else if (!string.IsNullOrEmpty(artworkUrl))
                {
                    song.Album.HasArtwork = await this.GetArtworkAsync(albumFilePath, artworkUrl);
                    await this.sqlService.UpdateItemAsync(song.Album);
                }

                // set it
                await this.dispatcher.RunAsync(
                    () =>
                        {
                            var artworkPath = string.Format(this.artworkFilePath, song.Album.Id);
                            if (song.Album.HasArtwork)
                            {
                                var path = this.localFilePrefix + artworkPath;

                                song.Album.Artwork = this.bitmapFactory.CreateImage(new Uri(path));

                                if (this.ScaledImageSize != 0)
                                {
                                    song.Album.Artwork.SetDecodedPixel(this.ScaledImageSize);

                                    song.Album.MediumArtwork = this.bitmapFactory.CreateImage(new Uri(path));
                                    song.Album.MediumArtwork.SetDecodedPixel(this.ScaledImageSize / 2);

                                    song.Album.SmallArtwork = this.bitmapFactory.CreateImage(new Uri(path));
                                    song.Album.SmallArtwork.SetDecodedPixel(50);
                                }
                            }
                            else
                            {
                                song.Album.Artwork = this.missingArtwork;
                                song.Album.MediumArtwork = this.missingArtwork;
                                song.Album.SmallArtwork = this.missingArtwork;
                            }
                        });

                #endregion
            }

            song.AlbumId = song.Album.Id;

            #endregion

            await this.sqlService.InsertAsync(song);

            await this.dispatcher.RunAsync(
                () =>
                    {
                        #region Order album songs

                        var orderedAlbumSong = song.Album.Songs.ToList();
                        orderedAlbumSong.Add(song);
                        orderedAlbumSong = orderedAlbumSong.OrderBy(p => p.TrackNumber).ToList();

                        var index = orderedAlbumSong.IndexOf(song);
                        song.Album.Songs.Insert(index, song);

                        #endregion

                        #region Order artist songs

                        var orderedArtistSong = song.Artist.Songs.ToList();
                        orderedArtistSong.Add(song);
                        orderedArtistSong = orderedArtistSong.OrderBy(p => p.Name).ToList();

                        index = orderedArtistSong.IndexOf(song);
                        song.Artist.Songs.Insert(index, song);

                        #endregion

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

        private async Task<bool> GetArtworkAsync(string filePath, Stream stream)
        {
            try
            {
                using (
                    var fileStream =
                        await
                        (await StorageHelper.CreateFileAsync(filePath, option: CreationCollisionOption.ReplaceExisting))
                            .OpenAsync(FileAccess.ReadAndWrite))
                {
                    await stream.CopyToAsync(fileStream);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> GetArtworkAsync(string filePath, string artworkUrl)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    using (var stream = await client.GetStreamAsync(artworkUrl))
                    {
                        return await this.GetArtworkAsync(filePath, stream);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Some shit happened saving the artwork, here: " + e);
            }

            return false;
        }

        #region Playback Queue

        public async Task ClearQueueAsync()
        {
            if (this.PlaybackQueue.Count == 0)
            {
                return;
            }

            await this.bgSqlService.DeleteTableAsync<QueueSong>();

            this.lookupMap.Clear();
            await this.dispatcher.RunAsync(
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
                await this.dispatcher.RunAsync(() => this.ShufflePlaybackQueue.SwitchTo(unshuffle));

                for (var i = 0; i < unshuffle.Count; i++)
                {
                    var queueSong = unshuffle[i];

                    queueSong.ShufflePrevId = i == 0 ? 0 : unshuffle[i - 1].Id;

                    if (i + 1 < unshuffle.Count)
                    {
                        queueSong.ShuffleNextId = unshuffle[i + 1].Id;
                    }

                    await this.bgSqlService.UpdateItemAsync(queueSong);
                }
            }
        }

        public async Task<QueueSong> AddToQueueAsync(Song song, QueueSong position = null, bool shuffleInsert = true)
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
                    this.lookupMap.TryGetValue(next.PrevId, out prev);
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
                        this.lookupMap.TryGetValue(shuffleNext.ShufflePrevId, out shufflePrev);
                    }
                }
            }
            else
            {
                if (this.ShufflePlaybackQueue.Count > 1)
                {
                    shuffleIndex = rnd.Next(1, this.ShufflePlaybackQueue.Count - 1);
                    shuffleNext = this.ShufflePlaybackQueue.ElementAt(shuffleIndex);

                    this.lookupMap.TryGetValue(shuffleNext.ShufflePrevId, out shufflePrev);
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
            await this.bgSqlService.InsertAsync(newQueue).ConfigureAwait(false);

            if (next != null)
            {
                // Update the prev id of the queue that was replaced
                next.PrevId = newQueue.Id;
                await this.bgSqlService.UpdateItemAsync(next).ConfigureAwait(false);
            }

            if (prev != null)
            {
                // Update the next id of the previous tail
                prev.NextId = newQueue.Id;
                await this.bgSqlService.UpdateItemAsync(prev).ConfigureAwait(false);
            }

            if (shuffleNext != null)
            {
                shuffleNext.ShufflePrevId = newQueue.Id;
                await this.bgSqlService.UpdateItemAsync(shuffleNext).ConfigureAwait(false);
            }

            if (shufflePrev != null)
            {
                shufflePrev.ShuffleNextId = newQueue.Id;
                await this.bgSqlService.UpdateItemAsync(shufflePrev).ConfigureAwait(false);
            }

            // Add the new queue entry to the collection and map
            await this.dispatcher.RunAsync(
                () =>
                    {
                        if (insert)
                        {
                            this.PlaybackQueue.Insert(normalIndex, newQueue);
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
                            this.ShufflePlaybackQueue.Insert(shuffleIndex, newQueue);
                        }
                    }).ConfigureAwait(false);

            this.lookupMap.TryAdd(newQueue.Id, newQueue);

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

            if (this.lookupMap.TryGetValue(songToRemove.PrevId, out previousModel))
            {
                previousModel.NextId = songToRemove.NextId;
                await this.bgSqlService.UpdateItemAsync(previousModel);
            }

            if (this.lookupMap.TryGetValue(songToRemove.ShufflePrevId, out previousModel))
            {
                previousModel.ShuffleNextId = songToRemove.ShuffleNextId;
                await this.bgSqlService.UpdateItemAsync(previousModel);
            }

            QueueSong nextModel;

            if (this.lookupMap.TryGetValue(songToRemove.NextId, out nextModel))
            {
                nextModel.PrevId = songToRemove.PrevId;
                await this.bgSqlService.UpdateItemAsync(nextModel);
            }

            if (this.lookupMap.TryGetValue(songToRemove.ShuffleNextId, out nextModel))
            {
                nextModel.ShufflePrevId = songToRemove.ShufflePrevId;
                await this.bgSqlService.UpdateItemAsync(nextModel);
            }

            await this.dispatcher.RunAsync(
                () =>
                    {
                        this.PlaybackQueue.Remove(songToRemove);
                        this.CurrentPlaybackQueue.Remove(songToRemove);
                    });
            this.lookupMap.TryRemove(songToRemove.Id, out songToRemove);

            // Delete from database
            await this.bgSqlService.DeleteItemAsync(songToRemove);
        }

        private void LoadQueue(List<Song> songs)
        {
            var queue = this.bgSqlService.SelectAll<QueueSong>();
            QueueSong head = null;
            QueueSong shuffleHead = null;

            foreach (var queueSong in queue)
            {
                queueSong.Song = songs.FirstOrDefault(p => p.Id == queueSong.SongId);

                this.lookupMap.TryAdd(queueSong.Id, queueSong);

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
                    if (this.dispatcher != null)
                    {
                        this.dispatcher.RunAsync(() => this.PlaybackQueue.Add(head)).Wait();
                    }
                    else
                    {
                        this.PlaybackQueue.Add(head);
                    }

                    if (head.NextId != 0 && this.lookupMap.ContainsKey(head.NextId))
                    {
                        head = this.lookupMap[head.NextId];
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
                    if (this.dispatcher != null)
                    {
                        this.dispatcher.RunAsync(() => this.ShufflePlaybackQueue.Add(shuffleHead)).Wait();
                    }
                    else
                    {
                        this.ShufflePlaybackQueue.Add(shuffleHead);
                    }

                    if (shuffleHead.ShuffleNextId != 0 && this.lookupMap.ContainsKey(shuffleHead.ShuffleNextId))
                    {
                        shuffleHead = this.lookupMap[shuffleHead.ShuffleNextId];
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
            await this.sqlService.InsertAsync(playlist);

            this.Playlists.Insert(0, playlist);

            return playlist;
        }

        public async Task DeletePlaylistAsync(Playlist playlist)
        {
            await this.sqlService.DeleteItemAsync(playlist);
            await this.sqlService.DeleteWhereAsync(playlist);
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
            await this.sqlService.InsertAsync(newSong);

            if (tail != null)
            {
                // Update the next id of the previous tail
                tail.NextId = newSong.Id;
                await this.sqlService.UpdateItemAsync(tail);
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
                        await this.sqlService.UpdateItemAsync(prevSong);
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
                        await this.sqlService.UpdateItemAsync(nextSong);
                    }
                }
            }

            

            #region update surrounding songs

            if (songPrevId != 0)
            {
                PlaylistSong prevSong;
                if (playlist.LookupMap.TryGetValue(songPrevId, out prevSong))
                {
                    prevSong.NextId = songNextId;
                    await this.sqlService.UpdateItemAsync(prevSong);
                }
            }

            if (songNextId != 0)
            {
                PlaylistSong nextSong;
                if (playlist.LookupMap.TryGetValue(songNextId, out nextSong))
                {
                    nextSong.PrevId = songPrevId;
                    await this.sqlService.UpdateItemAsync(nextSong);
                }
            }

            #endregion

            await this.sqlService.UpdateItemAsync(song);
            await this.sqlService.UpdateItemAsync(originalSong);
        }

        public async Task DeleteFromPlaylistAsync(Playlist playlist, PlaylistSong songToRemove)
        {
            

            if (songToRemove.NextId != 0)
            {
                var nextSong = playlist.LookupMap[songToRemove.NextId];
                nextSong.PrevId = songToRemove.PrevId;
                await this.sqlService.UpdateItemAsync(nextSong);
            }

            if (songToRemove.PrevId != 0)
            {
                var prevSong = playlist.LookupMap[songToRemove.PrevId];
                prevSong.NextId = songToRemove.NextId;
                await this.sqlService.UpdateItemAsync(prevSong);
            }

            

            await this.sqlService.DeleteItemAsync(songToRemove);
            await this.dispatcher.RunAsync(() => playlist.Songs.Remove(songToRemove));
        }

        private async void LoadPlaylists()
        {
            var playlists = this.sqlService.SelectAll<Playlist>().OrderByDescending(p => p.Id);
            var playlistSongs = this.sqlService.SelectAll<PlaylistSong>();

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

                

                if (this.dispatcher != null)
                {
                    await this.dispatcher.RunAsync(() => this.Playlists.Add(playlist));
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