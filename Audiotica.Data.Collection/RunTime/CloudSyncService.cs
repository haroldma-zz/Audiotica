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

        private readonly MobileServiceClient _mobileServiceClient;

        private readonly ISqlService _sqlService;

        public CloudSyncService(
            MobileServiceClient mobileServiceClient, 
            ICollectionService collectionService, 
            IAppSettingsHelper appSettingsHelper, 
            ISqlService sqlService)
        {
            _mobileServiceClient = mobileServiceClient;
            _collectionService = collectionService;
            _appSettingsHelper = appSettingsHelper;
            _sqlService = sqlService;
        }

        public event EventHandler AddedSong;

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
            await PullSyncNewSongsAsync(onlineSongs);
            await PullSyncDeletedSongsAsync(onlineSongs);
        }

        public async Task SyncAsync()
        {
            // Start by getting all cloud songs
            var onlineSongs = await _mobileServiceClient.GetTable<CloudSong>().ToListAsync().ConfigureAwait(false);
            var onlineArtists = await _mobileServiceClient.GetTable<CloudArtist>().ToListAsync().ConfigureAwait(false);
            var onlineAlbums = await _mobileServiceClient.GetTable<CloudAlbum>().ToListAsync().ConfigureAwait(false);

            foreach (var onlineAlbum in onlineAlbums)
            {
                onlineAlbum.PrimaryArtist = onlineArtists.FirstOrDefault(p => p.Id == onlineAlbum.PrimaryArtistId);
            }

            foreach (var onlineSong in onlineSongs)
            {
                onlineSong.Artist = onlineArtists.FirstOrDefault(p => p.Id == onlineSong.ArtistId);
                onlineSong.Album = onlineAlbums.FirstOrDefault(p => p.Id == onlineSong.AlbumId);
            }

            await PullAsync(onlineSongs);
            await PushAsync(onlineSongs, onlineArtists, onlineAlbums);
        }

        /// <summary>
        /// Pushes changes to the cloud.
        /// </summary>
        /// <param name="onlineSongs">The online songs.</param>
        /// <param name="onlineArtists">The online artists.</param>
        /// <param name="onlineAlbums">The online albums.</param>
        /// <returns>Task.</returns>
        public async Task PushAsync(
            List<CloudSong> onlineSongs,
            List<CloudArtist> onlineArtists,
            List<CloudAlbum> onlineAlbums)
        {
            await PushSyncNewSongsAsync(onlineSongs, onlineArtists, onlineAlbums);
            await PushSyncDeletedSongsAsync(onlineSongs);
        }

        /// <summary>
        /// Synchronizes the deleted online songs.
        /// </summary>
        /// <param name="onlineSongs">The cloud online songs.</param>
        /// <returns>Task.</returns>
        private async Task PullSyncDeletedSongsAsync(IEnumerable<CloudSong> onlineSongs)
        {
            var collectionSongs = _collectionService.Songs.Where(p => p.CloudId != null && p.SongState != SongState.Local).ToList();

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
        /// <param name="onlineSongs">The online songs.</param>
        /// <returns>Task.</returns>
        private async Task PullSyncNewSongsAsync(IEnumerable<CloudSong> onlineSongs)
        {
            var collectionSongs = _collectionService.Songs.Where(p => p.SongState != SongState.Local).ToList();

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
                        await _mobileServiceClient.GetTable<CloudSong>().DeleteAsync(onlineSong);
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
                    OnAddedSong(newSong);
                }
            }
        }

        /// <summary>
        /// Pushes the synchronize deleted songs asynchronous.
        /// </summary>
        /// <param name="onlineSongs">The online songs.</param>
        /// <returns>Task.</returns>
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
        /// <param name="onlineSongs">The online songs.</param>
        /// <param name="onlineArtists">The online artists.</param>
        /// <param name="onlineAlbums">The online albums.</param>
        /// <returns>Task.</returns>
        private async Task PushSyncNewSongsAsync(
            List<CloudSong> onlineSongs, 
            List<CloudArtist> onlineArtists, 
            List<CloudAlbum> onlineAlbums)
        {
            var syncTime = DateTime.UtcNow;

            var collectionSongs = _collectionService.Songs.Where(p => p.SongState != SongState.Local && p.CloudId == null).ToList();

            foreach (var collectionSong in collectionSongs)
            {
                var cloudSong = onlineSongs.FirstOrDefault(p => p.ProviderId == collectionSong.ProviderId);
                if (cloudSong != null)
                {
                    continue;
                }

                cloudSong = new CloudSong
                {
                    Name = collectionSong.Name, 
                    ArtistName = collectionSong.ArtistName, 
                    Duration = collectionSong.Duration, 
                    HeartState = collectionSong.HeartState, 
                    CreatedAt = syncTime, 
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
                        await _mobileServiceClient.GetTable<CloudArtist>().InsertAsync(cloaudAlbumArtist).ConfigureAwait(false);
                    }

                    cloudAlbum = new CloudAlbum
                    {
                        Name = collectionSong.Album.Name, 
                        ProviderId = collectionSong.Album.ProviderId, 
                        Genre = collectionSong.Album.Genre, 
                        ReleaseDate = collectionSong.Album.ReleaseDate, 
                        PrimaryArtistId = cloaudAlbumArtist.Id
                    };
                    await _mobileServiceClient.GetTable<CloudAlbum>().InsertAsync(cloudAlbum);
                }

                cloudSong.ArtistId = cloudArtist.Id;
                cloudSong.AlbumId = cloudAlbum.Id;

                await _mobileServiceClient.GetTable<CloudSong>().InsertAsync(cloudSong).ConfigureAwait(false);
                collectionSong.CloudId = cloudSong.Id;
                await _sqlService.UpdateItemAsync(collectionSong).ConfigureAwait(false);
            }

            LastSyncTime = syncTime;
        }

        protected virtual void OnAddedSong(Song song)
        {
            var handler = AddedSong;
            if (handler != null)
            {
                handler(song, EventArgs.Empty);
            }
        }
    }
}