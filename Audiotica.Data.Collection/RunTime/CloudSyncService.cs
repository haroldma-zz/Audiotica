using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Audiotica.Core.Utils.Interfaces;
using Audiotica.Data.Collection.Model;

using Microsoft.WindowsAzure.MobileServices;

namespace Audiotica.Data.Collection.RunTime
{
    public class CloudSyncService
    {
        private readonly IAppSettingsHelper _appSettingsHelper;

        private readonly ICollectionService _collectionService;

        private readonly IDispatcherHelper _dispatcherHelper;

        private readonly MobileServiceClient _mobileServiceClient;

        private readonly ISqlService _sqlService;

        public CloudSyncService(
            MobileServiceClient mobileServiceClient, 
            ICollectionService collectionService, 
            IAppSettingsHelper appSettingsHelper, 
            ISqlService sqlService, 
            IDispatcherHelper dispatcherHelper)
        {
            _mobileServiceClient = mobileServiceClient;
            _collectionService = collectionService;
            _appSettingsHelper = appSettingsHelper;
            _sqlService = sqlService;
            _dispatcherHelper = dispatcherHelper;
        }

        public event EventHandler OnLargeSyncStarted;

        public event EventHandler OnLargeSyncFinished;

        public DateTime LastSyncTime
        {
            get
            {
                return _appSettingsHelper.ReadJsonAs<DateTime>("LastSyncTime");
            }

            set
            {
                _appSettingsHelper.WriteAsJson("LastSyncTime", value);
            }
        }

        public async Task PullAsync(List<CloudSong> onlineSongs)
        {
            await PullSyncNewSongsAsync(onlineSongs).ConfigureAwait(false);
            await PullSyncDeletedSongsAsync(onlineSongs).ConfigureAwait(false);
        }

        /// <summary>
        /// Pushes changes to the cloud.
        /// </summary>
        /// <param name="onlineSongs">
        /// The online songs.
        /// </param>
        /// <param name="onlineArtists">
        /// The online artists.
        /// </param>
        /// <param name="onlineAlbums">
        /// The online albums.
        /// </param>
        /// <returns>
        /// Task.
        /// </returns>
        public async Task PushAsync(
            List<CloudSong> onlineSongs, 
            List<CloudArtist> onlineArtists, 
            List<CloudAlbum> onlineAlbums)
        {
            await PushSyncNewSongsAsync(onlineSongs, onlineArtists, onlineAlbums).ConfigureAwait(false);
            await PushSyncDeletedSongsAsync(onlineSongs).ConfigureAwait(false);
        }

        public async Task SyncAsync()
        {
            // Start by getting all cloud songs
            bool hasMore = true;
            var onlineSongs = new List<CloudSong>();
            var onlineArtists = new List<CloudArtist>();
            var onlineAlbums = new List<CloudAlbum>();

            while (hasMore)
            {
                var resultEnumerable =
                    (IQueryResultEnumerable<CloudSong>)
                    await
                    _mobileServiceClient.GetTable<CloudSong>()
                        .Skip(onlineSongs.Count)
                        .IncludeTotalCount()
                        .ToListAsync()
                        .ConfigureAwait(false);
                onlineSongs.AddRange(resultEnumerable);
                int totalCount = (int)resultEnumerable.TotalCount;
                hasMore = totalCount > onlineSongs.Count;
            }

            hasMore = true;
            while (hasMore)
            {
                var resultEnumerable =
                    (IQueryResultEnumerable<CloudArtist>)
                    await
                    _mobileServiceClient.GetTable<CloudArtist>()
                        .Skip(onlineArtists.Count)
                        .IncludeTotalCount()
                        .ToListAsync()
                        .ConfigureAwait(false);
                onlineArtists.AddRange(resultEnumerable);
                int totalCount = (int)resultEnumerable.TotalCount;
                hasMore = totalCount > onlineArtists.Count;
            }

            hasMore = true;
            while (hasMore)
            {
                var resultEnumerable =
                    (IQueryResultEnumerable<CloudAlbum>)
                    await
                    _mobileServiceClient.GetTable<CloudAlbum>()
                        .Skip(onlineAlbums.Count)
                        .IncludeTotalCount()
                        .ToListAsync()
                        .ConfigureAwait(false);
                onlineAlbums.AddRange(resultEnumerable);
                int totalCount = (int)resultEnumerable.TotalCount;
                hasMore = totalCount > onlineAlbums.Count;
            }

            foreach (var onlineAlbum in onlineAlbums)
            {
                onlineAlbum.PrimaryArtist = onlineArtists.FirstOrDefault(p => p.Id == onlineAlbum.PrimaryArtistId);
            }

            foreach (var onlineSong in onlineSongs)
            {
                onlineSong.Artist = onlineArtists.FirstOrDefault(p => p.Id == onlineSong.ArtistId);
                onlineSong.Album = onlineAlbums.FirstOrDefault(p => p.Id == onlineSong.AlbumId);
            }

            await PullAsync(onlineSongs).ConfigureAwait(false);
            await PushAsync(onlineSongs, onlineArtists, onlineAlbums).ConfigureAwait(false);
        }

        /// <summary>
        /// Synchronizes the deleted online songs.
        /// </summary>
        /// <param name="onlineSongs">
        /// The cloud online songs.
        /// </param>
        /// <returns>
        /// Task.
        /// </returns>
        private async Task PullSyncDeletedSongsAsync(IEnumerable<CloudSong> onlineSongs)
        {
            var collectionSongs =
                _collectionService.Songs.Where(p => p.CloudId != null && p.SongState != SongState.Local).ToList();

            foreach (var song in from song in collectionSongs
                                 let cloudSong = onlineSongs.FirstOrDefault(p => p.Id == song.CloudId)
                                 where cloudSong == null
                                 select song)
            {
                // If the local song is not in the cloud delete it.
                await _collectionService.DeleteSongAsync(song).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Synchronizes the new songs.
        /// </summary>
        /// <param name="onlineSongs">
        /// The online songs.
        /// </param>
        /// <returns>
        /// Task.
        /// </returns>
        private async Task PullSyncNewSongsAsync(List<CloudSong> onlineSongs)
        {
            var collectionSongs = _collectionService.Songs.Where(p => p.SongState != SongState.Local).ToList();

            // nothing different
            if (onlineSongs.Count - collectionSongs.Count == 0)
            {
                return;
            }

            // Wrap everything in one transaction
            _sqlService.BeginTransaction();

            // when syncing over 20 new songs is best to supress the collection (avoiding unnecessary ui work)
            var reset = (onlineSongs.Count - collectionSongs.Count) > 20;
            if (reset)
            {
                await _dispatcherHelper.RunAsync(
                    () =>
                    {
                        if (OnLargeSyncStarted != null)
                        {
                            OnLargeSyncStarted(null, EventArgs.Empty);
                        }

                        _collectionService.Songs.SuppressEvents = true;
                        _collectionService.Artists.SuppressEvents = true;
                        _collectionService.Albums.SuppressEvents = true;
                    });
            }

            foreach (var onlineSong in from onlineSong in onlineSongs
                                       let localSong = collectionSongs.FirstOrDefault(p => p.CloudId == onlineSong.Id)
                                       where localSong == null
                                       select onlineSong)
            {
                if (LastSyncTime > onlineSong.CreatedAt)
                {
                    await _mobileServiceClient.GetTable<CloudSong>().DeleteAsync(onlineSong).ConfigureAwait(false);
                    continue;
                }

                // Cloud song is not in the collection, check if it was saved before syncing
                var collSong = collectionSongs.FirstOrDefault(p => p.ProviderId == onlineSong.ProviderId);
                if (collSong != null)
                {
                    collSong.CloudId = onlineSong.Id;
                    await _sqlService.UpdateItemAsync(collSong).ConfigureAwait(false);
                }
                else
                {
                    if (onlineSong.Album == null | onlineSong.Artist == null)
                    {
                        await _mobileServiceClient.GetTable<CloudSong>().DeleteAsync(onlineSong).ConfigureAwait(false);
                        continue;
                    }

                    if (onlineSong.Album.PrimaryArtist == null)
                    {
                        onlineSong.Album.PrimaryArtist = onlineSong.Artist;
                    }

                    // Can't change CloudSong to Song, so need to create a new song
                    var newSong = new Song(onlineSong)
                    {
                        Album = new Album(onlineSong.Album), 
                        Artist = new Artist(onlineSong.Artist)
                    };

                    // By setting it to just synced, the app knows it has to match an audio url to it
                    await _collectionService.AddSongAsync(newSong).ConfigureAwait(false);
                }
            }

            // commit the transaction
            _sqlService.Commit();
            if (reset)
            {
                await _dispatcherHelper.RunAsync(
                    () =>
                    {
                        if (OnLargeSyncFinished != null)
                        {
                            OnLargeSyncFinished(null, EventArgs.Empty);
                        }

                        _collectionService.Songs.Reset();
                        _collectionService.Albums.Reset();
                        _collectionService.Artists.Reset();
                    });
            }
        }

        /// <summary>
        /// Pushes the synchronize deleted songs asynchronous.
        /// </summary>
        /// <param name="onlineSongs">
        /// The online songs.
        /// </param>
        /// <returns>
        /// Task.
        /// </returns>
        private async Task PushSyncDeletedSongsAsync(IEnumerable<CloudSong> onlineSongs)
        {
            foreach (var onlineSong in from onlineSong in onlineSongs
                                       let collectionSong =
                                           _collectionService.Songs.FirstOrDefault(
                                               p => p.ProviderId == onlineSong.ProviderId)
                                       where collectionSong == null
                                       select onlineSong)
            {
                // Delete it, if it wasn't already
                if (onlineSong.Id != null)
                {
                    await _mobileServiceClient.GetTable<CloudSong>().DeleteAsync(onlineSong).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Pushes the synchronize new songs asynchronous.
        /// </summary>
        /// <param name="onlineSongs">
        /// The online songs.
        /// </param>
        /// <param name="onlineArtists">
        /// The online artists.
        /// </param>
        /// <param name="onlineAlbums">
        /// The online albums.
        /// </param>
        /// <returns>
        /// Task.
        /// </returns>
        private async Task PushSyncNewSongsAsync(
            List<CloudSong> onlineSongs, 
            List<CloudArtist> onlineArtists, 
            List<CloudAlbum> onlineAlbums)
        {
            var collectionSongs =
                _collectionService.Songs.Where(p => p.SongState != SongState.Local && p.CloudId == null).ToList();

            foreach (var collectionSong in collectionSongs)
            {
                var cloudSong = onlineSongs.FirstOrDefault(p => p.ProviderId == collectionSong.ProviderId);
                if (cloudSong != null)
                {
                    collectionSong.CloudId = cloudSong.Id;
                    await _sqlService.UpdateItemAsync(collectionSong).ConfigureAwait(false);
                    continue;
                }

                if (collectionSong.Artist == null || collectionSong.Album == null)
                {
                    // Delete the invalid song
                    await _collectionService.DeleteSongAsync(collectionSong);
                    continue;
                }

                cloudSong = new CloudSong
                {
                    Name = collectionSong.Name, 
                    CreatedAt = DateTime.UtcNow, 
                    ArtistName = collectionSong.ArtistName, 
                    Duration = collectionSong.Duration, 
                    HeartState = collectionSong.HeartState, 
                    LastPlayed = collectionSong.LastPlayed, 
                    PlayCount = collectionSong.PlayCount, 
                    ProviderId = collectionSong.ProviderId, 
                    TrackNumber = collectionSong.TrackNumber
                };

                // before adding the song, there must be an artist and album id assigned to it
                var cloudArtist = onlineArtists.FirstOrDefault(p => p.ProviderId == collectionSong.Artist.ProviderId);
                var cloudAlbum = onlineAlbums.FirstOrDefault(p => p.ProviderId == collectionSong.Album.ProviderId);

                if (cloudArtist == null)
                {
                    cloudArtist = new CloudArtist
                    {
                        Name = collectionSong.Artist.Name, 
                        ProviderId = collectionSong.Artist.ProviderId
                    };
                    await _mobileServiceClient.GetTable<CloudArtist>().InsertAsync(cloudArtist).ConfigureAwait(false);
                }

                if (cloudAlbum == null)
                {
                    var cloaudAlbumArtist = cloudArtist;

                    if (cloudArtist.ProviderId != collectionSong.Album.PrimaryArtist.ProviderId)
                    {
                        cloaudAlbumArtist = new CloudArtist
                        {
                            Name = collectionSong.Album.PrimaryArtist.Name, 
                            ProviderId = collectionSong.Album.PrimaryArtist.ProviderId
                        };
                        await
                            _mobileServiceClient.GetTable<CloudArtist>()
                                .InsertAsync(cloaudAlbumArtist)
                                .ConfigureAwait(false);
                    }

                    cloudAlbum = new CloudAlbum
                    {
                        Name = collectionSong.Album.Name, 
                        ProviderId = collectionSong.Album.ProviderId, 
                        Genre = collectionSong.Album.Genre, 
                        ReleaseDate = collectionSong.Album.ReleaseDate, 
                        PrimaryArtistId = cloaudAlbumArtist.Id
                    };
                    await _mobileServiceClient.GetTable<CloudAlbum>().InsertAsync(cloudAlbum).ConfigureAwait(false);
                }

                cloudSong.ArtistId = cloudArtist.Id;
                cloudSong.AlbumId = cloudAlbum.Id;

                await _mobileServiceClient.GetTable<CloudSong>().InsertAsync(cloudSong).ConfigureAwait(false);
                collectionSong.CloudId = cloudSong.Id;
                await _sqlService.UpdateItemAsync(collectionSong).ConfigureAwait(false);
            }

            LastSyncTime = DateTime.UtcNow;
        }
    }
}