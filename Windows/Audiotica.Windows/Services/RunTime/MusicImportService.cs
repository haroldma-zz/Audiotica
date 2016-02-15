using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Services.Interfaces;
using TagLib;
using File = TagLib.File;

namespace Audiotica.Windows.Services.RunTime
{
    public class MusicImportService : IMusicImportService
    {
        private readonly ILibraryService _libraryService;
        private readonly ITrackSaveService _trackSaveService;

        public MusicImportService(ILibraryService libraryService, ITrackSaveService trackSaveService)
        {
            _libraryService = libraryService;
            _trackSaveService = trackSaveService;
        }

        public async Task SaveAsync(StorageFile file)
        {
            var audioPath = file.Path;

            if (_libraryService.Tracks.Any(p => p.AudioLocalUri == audioPath))
            {
                return;
            }

            #region id3 tags

            var tryAsM4A = false;
            var fileStream = await file.OpenStreamForReadAsync();
            File tagFile = null;
            try
            {
                tagFile = File.Create(new StreamFileAbstraction(file.Name, fileStream, fileStream));
            }
            catch (Exception e)
            {
                tryAsM4A = e is CorruptFileException;
            }

            if (tryAsM4A)
            {
                //need to reopen (when it fails to open it disposes of the stream
                fileStream = await file.OpenStreamForReadAsync();
                try
                {
                    tagFile = File.Create(new StreamFileAbstraction(
                        file.Name.Replace(".mp3", ".m4a"),
                        fileStream,
                        fileStream));
                }
                catch
                {
                    // ignored
                }
            }

            var tags = tagFile?.Tag;
            fileStream.Dispose();
            tagFile?.Dispose();

            #endregion

            var prop = await file.Properties.GetMusicPropertiesAsync().AsTask().ConfigureAwait(false);
            var track = tags == null ? CreateTrack(prop, audioPath) : CreateTrack(prop, tags, audioPath);

            //if no metadata was obtain, then result to using the filename
            if (string.IsNullOrEmpty(track.Title))
            {
                track.Title = file.DisplayName;
            }

            if (string.IsNullOrEmpty(track.AlbumArtist))
            {
                track.AlbumArtist = tags?.FirstAlbumArtist;
            }

            if (_libraryService.Find(track) != null)
            {
                return;
            }

            await _trackSaveService.SaveAsync(track, tags).ConfigureAwait(false);
        }

        public async Task<List<StorageFile>> ScanFolderAsync(IStorageFolder folder)
        {
            var audioFiles = new List<StorageFile>();

            //scan music folder
            await RetriveFilesInFolder(audioFiles, folder);

            return audioFiles;
        }

        protected Track CreateTrack(MusicProperties musicProps, Tag tag, string audioUrl)
        {
            return new Track
            {
                Title = CleanText(tag.Title),
                AlbumTitle = CleanText(tag.Album),
                AlbumArtist = CleanText(tag.FirstAlbumArtist) ?? CleanText(tag.FirstPerformer),
                Artists = CleanText(tag.JoinedPerformers),
                DisplayArtist = CleanText(tag.FirstPerformer),
                Bitrate = (int)musicProps.Bitrate,
                Duration = musicProps.Duration,
                Genres = tag.JoinedGenres,
                Publisher = musicProps.Publisher,
                Lyrics = tag.Lyrics,
                Comment = tag.Comment,
                Conductors = tag.Conductor,
                Composers = tag.JoinedComposers,
                TrackNumber = tag.Track,
                TrackCount = tag.TrackCount,
                DiscCount = tag.DiscCount,
                DiscNumber = tag.Disc,
                Year = tag.Year,
                LikeState = musicProps.Rating > 0 ? LikeState.Like : LikeState.None,
                AudioLocalUri = audioUrl,
                Type = TrackType.Local
            };
        }

        protected Track CreateTrack(MusicProperties musicProps, string audioUrl)
        {
            return new Track
            {
                Title = CleanText(musicProps.Title),
                AlbumTitle = CleanText(musicProps.Album),
                AlbumArtist = CleanText(musicProps.AlbumArtist) ?? CleanText(musicProps.Artist),
                Artists = CleanText(musicProps.Artist),
                DisplayArtist = CleanText(musicProps.Artist),
                Bitrate = (int)musicProps.Bitrate,
                Duration = musicProps.Duration,
                Genres = string.Join(";", musicProps.Genre),
                Publisher = musicProps.Publisher,
                Conductors = string.Join(";", musicProps.Conductors),
                Composers = string.Join(";", musicProps.Composers),
                TrackNumber = musicProps.TrackNumber,
                Year = musicProps.Year,
                LikeState = musicProps.Rating > 0 ? LikeState.Like : LikeState.None,
                AudioLocalUri = audioUrl,
                Type = TrackType.Local
            };
        }

        private string CleanText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            //[^0-9a-zA-Z]+
            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        private async Task RetriveFilesInFolder(List<StorageFile> list, IStorageFolder parent)
        {
            list.AddRange((await parent.GetFilesAsync()).Where(p =>
                p.FileType == ".wma"
                    || p.FileType == ".flac"
                    || p.FileType == ".m4a"
                    || p.FileType == ".mp3"));

            //avoiding DRM folder of xbox music
            foreach (var folder in (await parent.GetFoldersAsync()).Where(folder =>
                folder.Name != "Xbox Music" && folder.Name != "Subscription Cache" && folder.Name != "Podcasts"))
            {
                await RetriveFilesInFolder(list, folder);
            }
        }
    }
}