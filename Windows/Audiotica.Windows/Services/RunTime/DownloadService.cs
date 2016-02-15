using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Core.Windows.Helpers;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Common;
using Audiotica.Windows.Engine;
using Audiotica.Windows.Services.Interfaces;
using TagLib;
using File = TagLib.File;

namespace Audiotica.Windows.Services.RunTime
{
    public class SimpleFileAbstraction : File.IFileAbstraction
    {
        public SimpleFileAbstraction(string name, Stream readStream, Stream writeStream)
        {
            Name = name;
            ReadStream = readStream;
            WriteStream = writeStream;
        }

        public string Name { get; }

        public Stream ReadStream { get; }

        public Stream WriteStream { get; }

        public void CloseStream(Stream stream)
        {
            stream.Position = 0;
        }
    }

    public class DownloadService : IDownloadService
    {
        private readonly IAppSettingsUtility _appSettingsUtility;
        private readonly IPlayerService _playerService;
        private readonly IDispatcherUtility _dispatcherUtility;
        private readonly ILibraryService _libraryService;

        public DownloadService(
            ILibraryService libraryService,
            IDispatcherUtility dispatcherUtility,
            IAppSettingsUtility appSettingsUtility,
            IPlayerService playerService)
        {
            _libraryService = libraryService;
            _dispatcherUtility = dispatcherUtility;
            _appSettingsUtility = appSettingsUtility;
            _playerService = playerService;
            ActiveDownloads = new ObservableCollection<Track>();
        }

        public ObservableCollection<Track> ActiveDownloads { get; }

        public void Cancel(Track track)
        {
            Cancel(track.BackgroundDownload);
        }

        public void Cancel(BackgroundDownload backgroundDownload)
        {
            backgroundDownload.CancellationTokenSrc.Cancel();
        }

        public async void LoadDownloads()
        {
            await DiscoverActiveDownloadsAsync();
        }

        public void PauseAll()
        {
            foreach (var activeDownload in ActiveDownloads)
            {
                ((DownloadOperation)activeDownload.BackgroundDownload.DownloadOperation).Pause();
            }
        }

        public async Task StartDownloadAsync(Track track)
        {
            track.Status = TrackStatus.Downloading;

            try
            {
                // first create the final destination path (after download finishes)
                var downloadsPath = _appSettingsUtility.DownloadsPath;

                var path = track.AlbumArtist.ToSanitizedFileName() + "/" +
                    track.AlbumTitle.ToSanitizedFileName() + "/";
                var filename = track.Title.ToSanitizedFileName();

                if (track.DisplayArtist != track.AlbumArtist)
                {
                    filename = filename + "-" + track.DisplayArtist.ToSanitizedFileName();
                }

                track.AudioLocalUri = downloadsPath + path + filename + ".mp3";

                // now the temp file
                var saveFolder = await StorageFolder.GetFolderFromPathAsync(_appSettingsUtility.TempDownloadsPath);
                var tempFileName = $"{track.Id}.mp3";
                var destinationFile =
                    await
                        StorageHelper.CreateFileAsync(tempFileName, saveFolder, CreationCollisionOption.ReplaceExisting);

                // create the downloader
                var downloader = new BackgroundDownloader();
                var download = downloader.CreateDownload(new Uri(track.AudioWebUri), destinationFile);
                download.Priority = BackgroundTransferPriority.Default;

                // start handling it
                HandleDownload(track, download, true);

                // save changes to db
                await _libraryService.UpdateTrackAsync(track);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("there is not enough space on the disk"))
                {
                    CurtainPrompt.ShowError("Not enough disk space to download.");
                }
                track.Status = TrackStatus.None;
                await _libraryService.UpdateTrackAsync(track);
            }
        }

        #region Helpers

        private async Task DiscoverActiveDownloadsAsync()
        {
            //list containing all the operations (except grouped ones)
            IReadOnlyList<DownloadOperation> downloads;
            try
            {
                downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
                Debug.WriteLine("Found " + downloads.Count + " BackgroundDownload(s).");
            }
            catch
            {
                //failed silently
                return;
            }

            //no downloads? exit!
            if (downloads.Count == 0)
            {
                foreach (var track in _libraryService.Tracks.Where(p => p.Status == TrackStatus.Downloading).ToList())
                {
                    track.Status = TrackStatus.None;
                    await _libraryService.UpdateTrackAsync(track);
                }
                return;
            }

            foreach (var download in downloads)
            {
                //With the file name get the song
                var id = long.Parse(download.ResultFile.Name.Replace(download.ResultFile.FileType, ""));
                var songEntry = _libraryService.Tracks.FirstOrDefault(p => p.Id == id);

                if (songEntry != null)
                {
                    Debug.WriteLine("Handling downoad for {0}", songEntry);
                    HandleDownload(songEntry, download, false);
                }
            }

            // reset those without a bg download
            foreach (
                var track in
                    _libraryService.Tracks.Where(
                        p => p.Status == TrackStatus.Downloading && p.BackgroundDownload == null).ToList())
            {
                track.Status = TrackStatus.None;
                await _libraryService.UpdateTrackAsync(track);
            }
        }

        /// <summary>
        ///     Hanbdles a single BackgroundDownload for a song.
        /// </summary>
        private async void HandleDownload(Track track, DownloadOperation download, bool start)
        {
            track.BackgroundDownload = new BackgroundDownload(download);
            ActiveDownloads.Add(track);
            Debug.WriteLine("Added {0} to active downloads", track);

            try
            {
                var progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                if (start)
                {
                    // Start the BackgroundDownload and attach a progress handler.
                    await
                        download.StartAsync()
                            .AsTask(track.BackgroundDownload.CancellationTokenSrc.Token, progressCallback);
                }
                else
                {
                    // The BackgroundDownload was already running when the application started, re-attach the progress handler.
                    await
                        download.AttachAsync()
                            .AsTask(track.BackgroundDownload.CancellationTokenSrc.Token, progressCallback);
                }

                //Download Completed
                var response = download.GetResponseInformation();

                //Make sure it is success
                if (response.StatusCode < 400)
                {
                    await DownloadFinishedForAsync(track);
                }
                else
                {
                    Debug.WriteLine("Download status code for {0} is bad :/", track);
                    track.Status = TrackStatus.None;
                    await _libraryService.UpdateTrackAsync(track);
                    await ((DownloadOperation)track.BackgroundDownload.DownloadOperation).ResultFile.DeleteAsync();
                }
            }
            catch
            {
                Debug.WriteLine("Download cancelled {0}", track);

                track.AudioLocalUri = null;
                track.Status = TrackStatus.None;
                await _libraryService.UpdateTrackAsync(track);
                await ((DownloadOperation)track.BackgroundDownload.DownloadOperation).ResultFile.DeleteAsync();
            }
            finally
            {
                ActiveDownloads.Remove(track);
            }
        }

        /// <summary>
        ///     Use as callback of every downloads progress
        /// </summary>
        private void DownloadProgress(DownloadOperation download)
        {
            //Thread safety comes first!
            _dispatcherUtility.RunAsync(() =>
                {
                    //Get the associated song BackgroundDownload
                    var songDownload =
                        ActiveDownloads.FirstOrDefault(
                            p => ((DownloadOperation)p.BackgroundDownload.DownloadOperation).Guid == download.Guid);

                    if (songDownload == null)
                    {
                        return;
                    }

                    Debug.WriteLine("Updating song BackgroundDownload progress for {0}", songDownload);

                    songDownload.BackgroundDownload.Status =
                        download.Progress.Status.ToString().ToSentenceCase().Replace("Running", "Downloading");

                    if (download.Progress.TotalBytesToReceive > 0)
                    {
                        songDownload.BackgroundDownload.BytesToReceive = download.Progress.TotalBytesToReceive;
                        songDownload.BackgroundDownload.BytesReceived = download.Progress.BytesReceived;
                    }
                    else
                    {
                        songDownload.BackgroundDownload.Status = "Waiting";
                    }
                });
        }

        /// <summary>
        ///     Call internally to report a finished BackgroundDownload
        /// </summary>
        private async Task DownloadFinishedForAsync(Track track)
        {
            Debug.WriteLine("Download finished for {0}", track);
            var operation = (DownloadOperation)track.BackgroundDownload.DownloadOperation;
            var tempFile = operation.ResultFile;

            #region update id3 tags

            try
            {
                using (var fileStream = await tempFile.OpenStreamForWriteAsync())
                {
                    var tagFile = File.Create(new SimpleFileAbstraction(tempFile.Name, fileStream, fileStream));
                    tagFile.RemoveTags(TagTypes.Id3v2);
                    var newTags = tagFile.GetTag(TagTypes.Id3v2, true);

                    newTags.Title = track.Title;

                    newTags.Performers =
                        track.Artists.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(p => p.Trim())
                            .ToArray();

                    newTags.Album = track.AlbumTitle;
                    newTags.AlbumArtists = new[] { track.AlbumArtist };

                    if (!string.IsNullOrEmpty(track.Genres))
                    {
                        newTags.Genres = track.Genres.Split(';').Select(p => p.Trim()).ToArray();
                    }

                    newTags.Track = (uint)track.TrackNumber;
                    newTags.TrackCount = (uint)track.TrackCount;
                    newTags.Disc = (uint)track.DiscNumber;
                    if (track.Year != null)
                    {
                        newTags.Year = (uint)track.Year.Value;
                    }

                    newTags.Comment = "Downloaded with Audiotica - http://audiotica.fm";

                    if (!string.IsNullOrEmpty(track.ArtworkUri) && !track.ArtworkUri.StartsWith("http"))
                    {
                        var artworkFile =
                            await StorageHelper.GetFileAsync(track.ArtworkUri.Replace("ms-appdata:///local/", ""));

                        using (var artworkStream = await artworkFile.OpenStreamForReadAsync())
                        {
                            newTags.Pictures = new IPicture[]
                            {
                                new Picture(new SimpleFileAbstraction(artworkFile.Name, artworkStream, artworkStream))
                            };
                        }
                    }

                    await Task.Run(() => tagFile.Save());
                }
            }
            catch
            {
                // ignored
            }

            #endregion

            var newFile = await StorageHelper.CreateFileFromPathAsync(track.AudioLocalUri);
            await tempFile.MoveAndReplaceAsync(newFile);

            track.Status = TrackStatus.None;
            track.Type = TrackType.Download;
            track.BackgroundDownload = null;
            await _libraryService.UpdateTrackAsync(track);
            _playerService.UpdateUrl(track);
        }

        #endregion
    }
}