#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Media.Imaging;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection.Model;
using TagLib;

#endregion

namespace Audiotica.Data.Collection.RunTime
{
    public class CollectionService : INotifyPropertyChanged, ICollectionService
    {
        private readonly ISqlService _bgSqlService;
        private readonly CoreDispatcher _dispatcher;
        private readonly Dictionary<long, QueueSong> _lookupMap = new Dictionary<long, QueueSong>();
        private readonly ISqlService _sqlService;
        private int _scaledImageSize;

        public CollectionService(ISqlService sqlService, ISqlService bgSqlService, CoreDispatcher dispatcher)
        {
            _bgSqlService = bgSqlService;
            _sqlService = sqlService;
            _dispatcher = dispatcher;
            Songs = new OptimizedObservableCollection<Song>();
            Artists = new OptimizedObservableCollection<Artist>();
            Albums = new OptimizedObservableCollection<Album>();
            Playlists = new OptimizedObservableCollection<Playlist>();
            PlaybackQueue = new OptimizedObservableCollection<QueueSong>();
            ShufflePlaybackQueue = new OptimizedObservableCollection<QueueSong>();
        }

        private bool IsShuffle
        {
            get { return AppSettingsHelper.Read<bool>("Shuffle"); }
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
                if (IsShuffle)
                    return ShufflePlaybackQueue;
                return PlaybackQueue;
            }
        }

        public OptimizedObservableCollection<QueueSong> PlaybackQueue { get; private set; }
        public OptimizedObservableCollection<QueueSong> ShufflePlaybackQueue { get; private set; }

        public int ScaledImageSize
        {
            get
            {
                if (_scaledImageSize != 0)
                    return _scaledImageSize;

                _scaledImageSize = 200;
                double factor = 1;

                var scaledFactor = DisplayInformation.GetForCurrentView().ResolutionScale;
                switch (scaledFactor)
                {
                    case ResolutionScale.Scale120Percent:
                        factor = 1.2;
                        break;
                    case ResolutionScale.Scale140Percent:
                        factor = 1.4;
                        break;
                    case ResolutionScale.Scale150Percent:
                        factor = 1.5;
                        break;
                    case ResolutionScale.Scale160Percent:
                        factor = 1.6;
                        break;
                    case ResolutionScale.Scale180Percent:
                        factor = 1.8;
                        break;
                    case ResolutionScale.Scale225Percent:
                        factor = 2.25;
                        break;
                }

                _scaledImageSize = (int) (_scaledImageSize*factor);
                return _scaledImageSize;
            }
        }

        public void LoadLibrary(bool loadEssentials = false)
        {
            if (IsLibraryLoaded)
                return;

            /*
             * Sqlite makes a transaction to create a shared lock
             * Wrapping it in one single transactions assures it is only lock and release once
             */
            _sqlService.DbConnection.BeginTransaction();

            var songs = _sqlService.SelectAll<Song>().OrderByDescending(p => p.Id).ToList();
            var artists = _sqlService.SelectAll<Artist>().OrderByDescending(p => p.Id).ToList();
            var albums = new List<Album>();
            if (!loadEssentials)
                albums = _sqlService.SelectAll<Album>().OrderByDescending(p => p.Id).ToList();

            //Let go of the lock
            _sqlService.DbConnection.Commit();

            var isForeground = _dispatcher != null;

            foreach (var song in songs)
            {
                song.Artist = artists.FirstOrDefault(p => p.Id == song.ArtistId);
                song.Album = albums.FirstOrDefault(p => p.Id == song.AlbumId);
            }

            if (_dispatcher != null)
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    Songs.AddRange(songs));
            else
                Songs.AddRange(songs);

            foreach (var album in albums)
            {
                album.Songs.AddRange(songs.Where(p => p.AlbumId == album.Id).OrderBy(p => p.TrackNumber));
                album.PrimaryArtist = artists.FirstOrDefault(p => p.Id == album.PrimaryArtistId);

                if (isForeground)
                    _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        var artworkPath = string.Format(CollectionConstant.ArtworkPath, album.Id);
                        if (album.HasArtwork)
                        {
                            album.Artwork =
                                new BitmapImage(new Uri(CollectionConstant.LocalStorageAppPath + artworkPath))
                                {
                                    DecodePixelHeight = ScaledImageSize
                                };
                            album.MediumArtwork =
                                new BitmapImage(new Uri(CollectionConstant.LocalStorageAppPath + artworkPath))
                                {
                                    DecodePixelHeight = ScaledImageSize/2
                                };
                            album.SmallArtwork =
                                new BitmapImage(new Uri(CollectionConstant.LocalStorageAppPath + artworkPath))
                                {
                                    DecodePixelHeight = 50
                                };
                        }
                        else
                        {
                            album.Artwork = CollectionConstant.MissingArtworkImage;
                            album.MediumArtwork = CollectionConstant.MissingArtworkImage;
                            album.SmallArtwork = CollectionConstant.MissingArtworkImage;
                        }
                    }).AsTask().Wait();
            }

            if (_dispatcher != null)
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    Albums.AddRange(albums));
            else
                Albums.AddRange(albums);

            foreach (var artist in artists)
            {
                artist.Songs.AddRange(songs.Where(p => p.ArtistId == artist.Id).OrderBy(p => p.Name));
                artist.Albums.AddRange(albums.Where(p => p.PrimaryArtistId == artist.Id).OrderBy(p => p.Name));

                var songsAlbums = artist.Songs.Select(p => p.Album);
                artist.Albums.AddRange(songsAlbums.Where(p => !artist.Albums.Contains(p)));
                if (isForeground)
                    _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        var artworkPath = string.Format(CollectionConstant.ArtistsArtworkPath, artist.Id);
                        artist.Artwork = artist.HasArtwork
                            ? new BitmapImage(new Uri(CollectionConstant.LocalStorageAppPath + artworkPath))
                            {
                                DecodePixelHeight = ScaledImageSize
                            }
                            : null;
                    }).AsTask().Wait();
            }

            if (_dispatcher != null)
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    Artists.AddRange(artists));
            else
                Artists.AddRange(artists);


            IsLibraryLoaded = true;

            LoadQueue();

            if (!loadEssentials)
                LoadPlaylists();

            if (isForeground)
            {
                var corruptSongs =
                    Songs.Where(p => string.IsNullOrEmpty(p.Name) || p.Album == null || p.Artist == null).ToList();
                foreach (var corruptSong in corruptSongs)
                {
                    DeleteSongAsync(corruptSong).Wait();
                }

                _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (LibraryLoaded != null)
                        LibraryLoaded(this, null);
                });

                CleanupFiles();
            }
        }

        public Task LoadLibraryAsync(bool loadEssentials = false)
        {
            //just return non async as a task
            return Task.Factory.StartNew(() => LoadLibrary(loadEssentials));
        }

        public bool SongAlreadyExists(string localSongPath)
        {
            return Songs.FirstOrDefault(p =>
                (p.SongState == SongState.Local || p.SongState == SongState.Downloaded)
                && localSongPath == p.AudioUrl) != null;
        }

        public void ShuffleModeChanged()
        {
            OnPropertyChanged("CurrentPlaybackQueue");
        }

        public bool SongAlreadyExists(string providerId, string name, string album, string artist)
        {
            return Songs.FirstOrDefault(p => p.ProviderId == providerId
                                             || (p.Name == name && p.Album.Name == album && p.ArtistName == artist)) !=
                   null;
        }

        public Task AddSongAsync(Song song, string artworkUrl)
        {
            return AddSongAsync(song, null, artworkUrl);
        }

        public Task AddSongAsync(Song song, Tag tags)
        {
            return AddSongAsync(song, tags, null);
        }

        public async Task DeleteSongAsync(Song song)
        {
            var queueSong = PlaybackQueue.FirstOrDefault(p => p.SongId == song.Id);
            if (queueSong != null)
                await DeleteFromQueueAsync(queueSong);

            // remove it from artist, albums and playlists songs
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

            if (song.Album != null)
            {
                song.Album.Songs.Remove(song);
                if (song.Album.Songs.Count == 0)
                {
                    await _sqlService.DeleteItemAsync(song.Album);
                    await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Albums.Remove(song.Album);
                        song.Artist.Albums.Remove(song.Album);

                        var tileId = "album." + song.AlbumId;
                        if (SecondaryTile.Exists(tileId))
                        {
                            var secondaryTile = new SecondaryTile(tileId);
                            secondaryTile.RequestDeleteAsync();
                        }
                    });
                }
            }

            if (song.Artist != null)
            {
                song.Artist.Songs.Remove(song);
                if (song.Artist.Songs.Count == 0)
                {
                    await _sqlService.DeleteItemAsync(song.Artist);
                    await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Artists.Remove(song.Artist);
                        var tileId = "artist." + song.ArtistId;
                        if (SecondaryTile.Exists(tileId))
                        {
                            var secondaryTile = new SecondaryTile(tileId);
                            secondaryTile.RequestDeleteAsync();
                        }
                    });
                }
            }

            //good, now lets delete it from the db
            await _sqlService.DeleteItemAsync(song);

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Songs.Remove(song));
        }

        public async Task<List<HistoryEntry>> FetchHistoryAsync()
        {
            var list = await Task.FromResult(_bgSqlService.SelectAll<HistoryEntry>().ToList());
            foreach (var historyEntry in list)
            {
                historyEntry.Song = Songs.FirstOrDefault(p => p.Id == historyEntry.SongId);
            }
            return list;
        }

        private async Task AddSongAsync(Song song, Tag tags, string artworkUrl)
        {
            #region create artist

            var primaryArtist = (song.Album == null ? song.Artist : song.Album.PrimaryArtist) ?? new Artist
            {
                Name = "Unknown Artist",
                ProviderId = "autc.unknown"
            };

            var artist = Artists.FirstOrDefault(entry => entry.ProviderId == primaryArtist.ProviderId
                                                         ||
                                                         String.Equals(entry.Name, primaryArtist.Name,
                                                             StringComparison.CurrentCultureIgnoreCase));
            if (artist == null)
            {
                await _sqlService.InsertAsync(primaryArtist);
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    Artists.Insert(0, primaryArtist));

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

            #endregion

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

            var album = Albums.FirstOrDefault(p => p.ProviderId == song.Album.ProviderId);

            if (album != null)
                song.Album = album;
            else
            {
                await _sqlService.InsertAsync(song.Album);
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Albums.Insert(0, song.Album));

                #region Download artwork

                var albumFilePath = string.Format(CollectionConstant.ArtworkPath, song.Album.Id);

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
                        song.Album.HasArtwork = await GetArtworkAsync(albumFilePath, artwork);
                        await _sqlService.UpdateItemAsync(song.Album);
                        artwork.Dispose();
                    }
                }
                else if (!string.IsNullOrEmpty(artworkUrl))
                {
                    song.Album.HasArtwork = await GetArtworkAsync(albumFilePath, artworkUrl);
                    await _sqlService.UpdateItemAsync(song.Album);
                }

                //set it
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (song.Album.HasArtwork)
                    {
                        var artworkPath = string.Format(CollectionConstant.ArtworkPath, song.Album.Id);
                        song.Album.Artwork =
                            new BitmapImage(new Uri(CollectionConstant.LocalStorageAppPath + artworkPath))
                            {
                                DecodePixelHeight = ScaledImageSize
                            };
                        song.Album.MediumArtwork =
                            new BitmapImage(new Uri(CollectionConstant.LocalStorageAppPath + artworkPath))
                            {
                                DecodePixelHeight = ScaledImageSize/2
                            };
                        song.Album.SmallArtwork =
                            new BitmapImage(new Uri(CollectionConstant.LocalStorageAppPath + artworkPath))
                            {
                                DecodePixelHeight = 50
                            };
                    }
                    else
                    {
                        song.Album.Artwork = CollectionConstant.MissingArtworkImage;
                        song.Album.MediumArtwork = CollectionConstant.MissingArtworkImage;
                        song.Album.SmallArtwork = CollectionConstant.MissingArtworkImage;
                    }
                });

                #endregion
            }

            song.AlbumId = song.Album.Id;

            #endregion

            await _sqlService.InsertAsync(song);

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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

                Songs.Insert(0, song);
            });
        }

        private async Task<bool> GetArtworkAsync(string filePath, Stream stream)
        {
            try
            {
                using (var fileStream = await
                    (await StorageHelper.CreateFileAsync(filePath, option: CreationCollisionOption.ReplaceExisting))
                        .OpenStreamForWriteAsync())
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
                        return await GetArtworkAsync(filePath, stream);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Some shit happened saving the artwork, here: " + e);
            }
            return false;
        }

        private async void CleanupFiles()
        {
            var artworkFolder = await StorageHelper.GetFolderAsync("artworks");

            if (artworkFolder == null) return;

            var artworks = await artworkFolder.GetFilesAsync();

            foreach (var file in from file in artworks
                let id = file.Name.Replace(".jpg", "")
                where Albums.FirstOrDefault(p => p.Id.ToString() == id) == null
                      && Artists.FirstOrDefault(p => p.ProviderId == id) == null
                select file)
            {
                try
                {
                    await file.DeleteAsync();
                    Debug.WriteLine("Deleted file: {0}", file.Name);
                }
                catch
                {
                }
            }
        }

        #region Playback Queue

        public async Task ClearQueueAsync()
        {
            if (PlaybackQueue.Count == 0) return;
            await _bgSqlService.DeleteTableAsync<QueueSong>();

            _lookupMap.Clear();
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PlaybackQueue.Clear();
                ShufflePlaybackQueue.Clear();
            });
        }

        public async Task ShuffleCurrentQueueAsync()
        {
            var unshuffle = PlaybackQueue.ToList().Shuffle();

            if (unshuffle.Count >= 5)
            {
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ShufflePlaybackQueue.SwitchTo(unshuffle));

                for (var i = 0; i < unshuffle.Count; i++)
                {
                    var queueSong = unshuffle[i];

                    queueSong.ShufflePrevId = i == 0 ? 0 : unshuffle[i - 1].Id;

                    if (i + 1 < unshuffle.Count)
                        queueSong.ShuffleNextId = unshuffle[i + 1].Id;

                    await _bgSqlService.UpdateItemAsync(queueSong);
                }
            }
        }

        public async Task<QueueSong> AddToQueueAsync(Song song, QueueSong position = null, bool shuffleInsert = true)
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            QueueSong prev = null;
            QueueSong shufflePrev = null;
            QueueSong next = null;
            QueueSong shuffleNext = null;
            var shuffleIndex = -1;
            var normalIndex = -1;

            if (position != null)
            {
                shuffleIndex = ShufflePlaybackQueue.IndexOf(position) + 1;
                normalIndex = PlaybackQueue.IndexOf(position) + 1;
            }

            var insert = normalIndex > -1 && normalIndex < PlaybackQueue.Count;
            var insertShuffle = shuffleIndex > -1 && shuffleInsert;
            var shuffleLastAdd = shuffleIndex == ShufflePlaybackQueue.Count;

            if (insert)
            {
                next = PlaybackQueue[normalIndex];
                if (next.PrevId != 0 && _lookupMap.ContainsKey(next.PrevId))
                    prev = _lookupMap[next.PrevId];
            }
            else
            {
                prev = PlaybackQueue.LastOrDefault();
            }

            if (insertShuffle)
            {
                if (shuffleLastAdd)
                    shufflePrev = ShufflePlaybackQueue[ShufflePlaybackQueue.Count - 1];
                else
                {
                    shuffleNext = ShufflePlaybackQueue[shuffleIndex];
                    if (shuffleNext.ShufflePrevId != 0 && _lookupMap.ContainsKey(shuffleNext.ShufflePrevId))
                        shufflePrev = _lookupMap[shuffleNext.ShufflePrevId];
                }
            }
            else
            {
                if (ShufflePlaybackQueue.Count > 1)
                {
                    shuffleIndex = rnd.Next(1, ShufflePlaybackQueue.Count - 1);
                    shuffleNext = ShufflePlaybackQueue.ElementAt(shuffleIndex);

                    if (shuffleNext.ShufflePrevId != 0 && _lookupMap.ContainsKey(shuffleNext.ShufflePrevId))
                        shufflePrev = _lookupMap[shuffleNext.ShufflePrevId];
                }
                else
                {
                    shuffleIndex = PlaybackQueue.Count;
                    shufflePrev = prev;
                }
            }


            //Create the new queue entry
            var newQueue = new QueueSong
            {
                SongId = song.Id,
                NextId = next == null ? 0 : next.Id,
                PrevId = prev == null ? 0 : prev.Id,
                ShuffleNextId = shuffleNext == null ? 0 : shuffleNext.Id,
                ShufflePrevId = shufflePrev == null ? 0 : shufflePrev.Id,
                Song = song
            };

            //Add it to the database
            await _bgSqlService.InsertAsync(newQueue).ConfigureAwait(false);

            if (next != null)
            {
                //Update the prev id of the queue that was replaced
                next.PrevId = newQueue.Id;
                await _bgSqlService.UpdateItemAsync(next).ConfigureAwait(false);
            }

            if (prev != null)
            {
                //Update the next id of the previous tail
                prev.NextId = newQueue.Id;
                await _bgSqlService.UpdateItemAsync(prev).ConfigureAwait(false);
            }

            if (shuffleNext != null)
            {
                shuffleNext.ShufflePrevId = newQueue.Id;
                await _bgSqlService.UpdateItemAsync(shuffleNext).ConfigureAwait(false);
            }

            if (shufflePrev != null)
            {
                shufflePrev.ShuffleNextId = newQueue.Id;
                await _bgSqlService.UpdateItemAsync(shufflePrev).ConfigureAwait(false);
            }

            //Add the new queue entry to the collection and map
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (insert)
                    PlaybackQueue.Insert(normalIndex, newQueue);
                else
                    PlaybackQueue.Add(newQueue);

                if (shuffleLastAdd)
                    ShufflePlaybackQueue.Add(newQueue);
                else
                    ShufflePlaybackQueue.Insert(shuffleIndex, newQueue);

                if (_lookupMap.ContainsKey(newQueue.Id))
                    _lookupMap.Remove(newQueue.Id);

                _lookupMap.Add(newQueue.Id, newQueue);
            }).AsTask().ConfigureAwait(false);

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
                return;

            if (_lookupMap.TryGetValue(songToRemove.PrevId, out previousModel))
            {
                previousModel.NextId = songToRemove.NextId;
                await _bgSqlService.UpdateItemAsync(previousModel);
            }

            if (_lookupMap.TryGetValue(songToRemove.ShufflePrevId, out previousModel))
            {
                previousModel.ShuffleNextId = songToRemove.ShuffleNextId;
                await _bgSqlService.UpdateItemAsync(previousModel);
            }

            QueueSong nextModel;

            if (_lookupMap.TryGetValue(songToRemove.NextId, out nextModel))
            {
                nextModel.PrevId = songToRemove.PrevId;
                await _bgSqlService.UpdateItemAsync(nextModel);
            }

            if (_lookupMap.TryGetValue(songToRemove.ShuffleNextId, out nextModel))
            {
                nextModel.ShufflePrevId = songToRemove.ShufflePrevId;
                await _bgSqlService.UpdateItemAsync(nextModel);
            }

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PlaybackQueue.Remove(songToRemove);
                CurrentPlaybackQueue.Remove(songToRemove);
            });
            _lookupMap.Remove(songToRemove.Id);

            //Delete from database
            await _bgSqlService.DeleteItemAsync(songToRemove);
        }

        private void LoadQueue()
        {
            var queue = _bgSqlService.SelectAll<QueueSong>();
            QueueSong head = null;
            QueueSong shuffleHead = null;

            foreach (var queueSong in queue)
            {
                queueSong.Song = Songs.FirstOrDefault(p => p.Id == queueSong.SongId);

                if (_lookupMap.ContainsKey(queueSong.Id))
                    _lookupMap.Remove(queueSong.Id);

                _lookupMap.Add(queueSong.Id, queueSong);

                if (queueSong.ShufflePrevId == 0)
                    shuffleHead = queueSong;

                if (queueSong.PrevId == 0)
                    head = queueSong;
            }

            if (head != null)
            {
                for (var i = 0; i < queue.Count; i++)
                {
                    if (_dispatcher != null)
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => PlaybackQueue.Add(head))
                            .AsTask()
                            .Wait();
                    else
                        PlaybackQueue.Add(head);

                    if (head.NextId != 0 && _lookupMap.ContainsKey(head.NextId))
                        head = _lookupMap[head.NextId];
                    else
                        break;
                }
            }
            if (shuffleHead != null)
            {
                for (var i = 0; i < queue.Count; i++)
                {
                    if (_dispatcher != null)
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () => ShufflePlaybackQueue.Add(shuffleHead))
                            .AsTask()
                            .Wait();
                    else
                        ShufflePlaybackQueue.Add(shuffleHead);

                    if (shuffleHead.ShuffleNextId != 0 && _lookupMap.ContainsKey(shuffleHead.ShuffleNextId))
                        shuffleHead = _lookupMap[shuffleHead.ShuffleNextId];
                    else
                        break;
                }
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
            await _sqlService.DeleteWhereAsync(playlist);
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
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => playlist.Songs.Remove(songToRemove));
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}