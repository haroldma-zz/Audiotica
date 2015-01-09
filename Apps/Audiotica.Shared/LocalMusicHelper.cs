#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Audiotica.Core.Exceptions;
using Audiotica.Core.Utilities;
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
            foreach (var folder in (await parent.GetFoldersAsync()).Where(folder => 
                folder.Name != "Xbox Music" && folder.Name != "Subscription Cache" && folder.Name != "Podcasts"))
            {
                await RetriveFilesInFolder(list, folder);
            }
        }

        public static async Task<List<StorageFile>> GetFilesInMusic()
        {
            var audioFiles = new List<StorageFile>();

            //scan music folder
            await RetriveFilesInFolder(audioFiles, KnownFolders.MusicLibrary);

            return audioFiles;
        }

        public static Album ToAlbum(this LocalSong track)
        {
            return new Album
            {
                ProviderId = "local." + track.AlbumId,
                Name = track.AlbumName.Trim()
            };
        }

        public static Artist ToArtist(this LocalSong track)
        {
            return new Artist
            {
                ProviderId = "local." + track.ArtistId,
                Name = (string.IsNullOrEmpty(track.AlbumArtist) 
                ? track.ArtistName
                : track.AlbumArtist).Trim()
            };
        }

        public static Song ToSong(this LocalSong track)
        {
            var song = new Song
            {
                ProviderId = "local." + track.Id,
                Name = track.Title,
                ArtistName = track.ArtistName,
                Duration = track.Duration,
                AudioUrl = track.FilePath,
                SongState = SongState.Local,
                TrackNumber = track.TrackNumber,
                HeartState = track.HeartState
            };

            if (!string.IsNullOrEmpty(track.ArtistId))
            {
                song.Artist = track.ToArtist();
            }

            if (!string.IsNullOrEmpty(track.AlbumId))
            {
                song.Album = track.ToAlbum();
                song.Album.PrimaryArtist = song.Artist;
            }
            return song;
        }

        public static async Task<SavingError> SaveTrackAsync(StorageFile file)
        {
            var audioPath = file.Path.Substring(3)
                //local path
                .Replace(@"Data\Users\Public\Music\", "")
                //external path
                .Replace(@"Music\", "")
                //using forward slashes
                .Replace("\\", "/");
            
            if (App.Locator.CollectionService.SongAlreadyExists(audioPath))
            {
                return SavingError.AlreadyExists;
            }

            #region Getting metadata

            #region id3 tags

            File tagFile;
            var fileStream = await file.OpenStreamForReadAsync();
            try
            {
                tagFile = File.Create(new StreamFileAbstraction(file.Name, fileStream, fileStream));
            }
            catch
            {
                tagFile = null;
            }

            if (tagFile == null)
            {
                //need to reopen (when it fails to open it disposes of the stream
                fileStream = await file.OpenStreamForReadAsync();
                tagFile = File.Create(new StreamFileAbstraction(
                    file.Name.Replace(".mp3", ".m4a"), fileStream, fileStream));
            }

            var tags = tagFile.Tag;
            using (fileStream) { }
            using (tagFile) { }

            #endregion

            LocalSong track;
            if (tags == null)
            {
                //if there aren't any id3tags, try using the properties from the file
                var prop = await file.Properties.GetMusicPropertiesAsync().AsTask().ConfigureAwait(false);
                track = new LocalSong(prop)
                {
                    FilePath = audioPath
                };
            }
            else
            {
                track = new LocalSong(tags.Title, tags.JoinedPerformers, tags.Album, tags.FirstAlbumArtist)
                {
                    FilePath = audioPath,
                    Genre = tags.FirstGenre,
                    TrackNumber = (int)tags.Track,
                };
            }

            var song = track.ToSong();

            #endregion

            //if no metadata was obtain, then result to using the filename
            if (string.IsNullOrEmpty(song.Name))
            {
                song.Name = file.DisplayName;
                song.ProviderId += Convert.ToBase64String(Encoding.UTF8.GetBytes(song.Name.ToLower()));
            }


            if (App.Locator.CollectionService.SongAlreadyExists(song.ProviderId, 
                track.Title, track.AlbumName, track.ArtistName))
            {
                return SavingError.AlreadyExists;
            }

            try
            {
                await
                    App.Locator.CollectionService.AddSongAsync(song, tags, null)
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