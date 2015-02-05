﻿using System;
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

        public CloudSyncService(MobileServiceClient mobileServiceClient, ICollectionService collectionService,
                                IAppSettingsHelper appSettingsHelper, ISqlService sqlService)
        {
            _mobileServiceClient = mobileServiceClient;
            _collectionService = collectionService;
            _appSettingsHelper = appSettingsHelper;
            _sqlService = sqlService;
        }

        public DateTime LastSyncTime
        {
            get { return _appSettingsHelper.ReadJsonAs<DateTime>("LastSyncTime"); }
            set { _appSettingsHelper.WriteAsJson("LastSyncTime", value); }
        }

        public async Task PullAsync()
        {
            // Start by getting all cloud songs
            var onlineSongs = await _mobileServiceClient.GetTable<CloudSong>().ToListAsync();
            var onlineArtists = await _mobileServiceClient.GetTable<CloudArtist>().ToListAsync();
            var onlineAlbums = await _mobileServiceClient.GetTable<CloudAlbum>().ToListAsync();

            foreach (var onlineAlbum in onlineAlbums)
            {
                onlineAlbum.PrimaryArtist = onlineArtists.FirstOrDefault(p => p.Id == onlineAlbum.PrimaryArtistId);
            }

            foreach (var onlineSong in onlineSongs)
            {
                onlineSong.Artist = onlineArtists.FirstOrDefault(p => p.Id == onlineSong.ArtistId);
                onlineSong.Album = onlineAlbums.FirstOrDefault(p => p.Id == onlineSong.AlbumId);
            }

            await PullSyncNewSongsAsync(onlineSongs);
            await PullSyncDeletedSongsAsync(onlineSongs);
        }

        /// <summary>
        ///     Pushes changes to the cloud.
        /// </summary>
        /// <returns></returns>
        public async Task PushAsync()
        {
            // Start by getting all cloud songs
            var onlineSongs = await _mobileServiceClient.GetTable<CloudSong>().ToListAsync();
            var onlineArtists = await _mobileServiceClient.GetTable<CloudArtist>().ToListAsync();
            var onlineAlbums = await _mobileServiceClient.GetTable<CloudAlbum>().ToListAsync();

            await PushSyncNewSongsAsync(onlineSongs, onlineArtists, onlineAlbums);
            await PushSyncDeletedSongsAsync(onlineSongs);
        }

        /// <summary>
        ///     Synchronizes the deleted online songs.
        /// </summary>
        /// <param name="onlineSongs">The cloud online songs.</param>
        private async Task PullSyncDeletedSongsAsync(IEnumerable<CloudSong> onlineSongs)
        {
            var collectionSongs = _collectionService.Songs.Where(p => p.CloudId != null).ToList();

            foreach (var song in from song in collectionSongs
                                 let cloudSong = onlineSongs.FirstOrDefault(p => p.Id == song.CloudId)
                                 where cloudSong == null
                                 select song)
            {
                // If the local song is not in the cloud delete it.
                await _collectionService.DeleteSongAsync(song);
            }
        }

        /// <summary>
        ///     Synchronizes the new songs.
        /// </summary>
        /// <param name="onlineSongs">The online songs.</param>
        private async Task PullSyncNewSongsAsync(IEnumerable<CloudSong> onlineSongs)
        {
            var collectionSongs = _collectionService.Songs.ToList();

            foreach (var onlineSong in from onlineSong in onlineSongs
                                       let localSong = collectionSongs.FirstOrDefault(p => p.CloudId == onlineSong.Id)
                                       where localSong == null
                                       select onlineSong)
            {
                // Can't change CloudSong to Song, so need to create a new song
                var newSong = new Song(onlineSong) {Album = new Album(onlineSong.Album), Artist = new Artist(onlineSong.Artist)};

                // By setting it to just synced, the app knows it has to match an audio url to it
                await _collectionService.AddSongAsync(newSong);
            }
        }

        /// <summary>
        ///     Pushes the synchronize deleted songs asynchronous.
        /// </summary>
        /// <param name="onlineSongs">The online songs.</param>
        /// <returns></returns>
        private async Task PushSyncDeletedSongsAsync(IEnumerable<CloudSong> onlineSongs)
        {
            foreach (var onlineSong in from onlineSong in onlineSongs
                                       let collectionSong =
                                           _collectionService.Songs.FirstOrDefault(
                                               p => p.ProviderId == onlineSong.ProviderId)
                                       where collectionSong == null
                                       select onlineSong)
            {
                // Delete it
                await _mobileServiceClient.GetTable<CloudSong>().DeleteAsync(onlineSong);
            }
        }

        /// <summary>
        ///     Pushes the synchronize new songs asynchronous.
        /// </summary>
        /// <param name="onlineSongs">The online songs.</param>
        /// <param name="onlineArtists">The online artists.</param>
        /// <param name="onlineAlbums">The online albums.</param>
        /// <returns></returns>
        private async Task PushSyncNewSongsAsync(List<CloudSong> onlineSongs, List<CloudArtist> onlineArtists,
                                                 List<CloudAlbum> onlineAlbums)
        {
            var syncTime = DateTime.UtcNow;

            // Only push songs that have been added since the last sync
            var collectionSongs = _collectionService.Songs.Where(p => p.CreatedAt > LastSyncTime).ToList();

            foreach (var collectionSong in collectionSongs)
            {
                var cloudSong = onlineSongs.FirstOrDefault(p => p.ProviderId == collectionSong.ProviderId);
                if (cloudSong != null) continue;

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
                    await _mobileServiceClient.GetTable<CloudArtist>().InsertAsync(cloudArtist);
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
                        await _mobileServiceClient.GetTable<CloudArtist>().InsertAsync(cloaudAlbumArtist);
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

                await _mobileServiceClient.GetTable<CloudSong>().InsertAsync(cloudSong);
                collectionSong.CloudId = cloudSong.Id;
                await _sqlService.UpdateItemAsync(collectionSong);
            }

            LastSyncTime = syncTime;
        }
    }
}