#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Audiotica.Core.Exceptions;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Model;
using TagLib;

#endregion

namespace Audiotica
{
    public static class LocalMusicHelper
    {
        // first - a method to retrieve files from folder recursively 
        private static async Task RetriveFilesInFolder(List<StorageFile> list, StorageFolder parent)
        {
            list.AddRange((await parent.GetFilesAsync()).Where(p => p.FileType == ".wma" || p.FileType == ".mp3"));

            //avoiding DRM folder of xbox music
            foreach (var item in (await parent.GetFoldersAsync()).Where(item => !item.Path.Contains("Xbox Music")))
            {
                await RetriveFilesInFolder(list, item);
            }
        }

        public static async Task<List<StorageFile>> GetFilesInMusic()
        {
            var folder = KnownFolders.MusicLibrary;
            var audioFiles = new List<StorageFile>();
            await RetriveFilesInFolder(audioFiles, folder);
            return audioFiles;
        }

        public static Album ToAlbum(this LocalSong track)
        {
            return new Album
            {
                ProviderId = "local." + track.AlbumId,
                Name = track.AlbumName.Trim().Replace("  ", " ")
            };
        }

        public static Artist ToArtist(this LocalSong track)
        {
            return new Artist
            {
                ProviderId = "local." + track.ArtistId,
                Name = track.ArtistName.Trim().Replace("  ", " ")
            };
        }

        public static Song ToSong(this LocalSong track)
        {
            var song = new Song
            {
                ProviderId = "local." + track.Id,
                Name = track.Title.Trim().Replace("  ", " "),
                ArtistName = track.ArtistName,
                Duration = track.Duration,
                AudioUrl = track.FilePath,
                SongState = SongState.Local,
                TrackNumber = track.TrackNumber,
                HeartState = track.HeartState,
                Artist = track.ToArtist(),
            };

            if (!string.IsNullOrEmpty(track.AlbumId))
            {
                song.Album = track.ToAlbum();
                song.Album.PrimaryArtist = song.Artist;
            }
            return song;
        }

        public static async Task<SavingError> SaveTrackAsync(StorageFile file)
        {
            var audioPath = file.Path.Replace(@"C:\Data\Users\Public\Music", "").Replace("\\", "/");
            
            if (App.Locator.CollectionService.SongAlreadyExists(audioPath))
            {
                return SavingError.AlreadyExists;
            }

            var prop = await file.Properties.GetMusicPropertiesAsync();
            var track = new LocalSong(prop)
            {
                FilePath = audioPath
            };
            var song = track.ToSong();

            if (App.Locator.CollectionService.SongAlreadyExists(song.ProviderId, 
                track.Title, track.AlbumName, track.ArtistName))
            {
                return SavingError.AlreadyExists;
            }

            string artistArtwork = null;

            if (!string.IsNullOrEmpty(song.ArtistName) 
                && App.Locator.CollectionService.Artists.FirstOrDefault(p =>
                String.Equals(p.Name, song.ArtistName, StringComparison.CurrentCultureIgnoreCase)) == null)
            {
                try
                {
                    var lastArtist = await App.Locator.ScrobblerService.GetDetailArtist(song.ArtistName);
                    artistArtwork = lastArtist.MainImage != null && lastArtist.MainImage.Largest != null
                        ? lastArtist.MainImage.Largest.AbsoluteUri
                        : null;
                }
                catch
                {
                }
            }

            try
            {
                await
                    App.Locator.CollectionService.AddSongAsync(song, file, artistArtwork)
                        .ConfigureAwait(false);
                return SavingError.None;
            }
            catch (NetworkException)
            {
                return SavingError.Network;
            }
            catch
            {
                return SavingError.Unknown;
            }
        }
    }
}