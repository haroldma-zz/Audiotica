#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Core;
using Audiotica.Core.Utils;
using Audiotica.Core.WinRt;
using Audiotica.Core.WinRt.Common;
using Audiotica.Core.WinRt.Utilities;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using PCLStorage;
using TagLib;
using CreationCollisionOption = Windows.Storage.CreationCollisionOption;

#endregion

namespace Audiotica
{
    public class SongDownloadService : ISongDownloadService
    {
        #region Constructor

        public SongDownloadService(ICollectionService service, ISqlService sqlService, CoreDispatcher dispatcher)
        {
            _service = service;
            _sqlService = sqlService;
            _dispatcher = dispatcher;
        }

        #endregion

        #region Private Fields

        private readonly CoreDispatcher _dispatcher;
        private readonly ICollectionService _service;
        private readonly ISqlService _sqlService;

        #endregion

        #region Helper methods

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
            if (downloads.Count == 0) return;

            foreach (var download in downloads)
            {
                //With the uri get the song
                var songEntry = _service.Songs.FirstOrDefault(p => p.DownloadId == download.Guid.ToString());

                if (songEntry != null)
                {
                    Debug.WriteLine("Handling downoad for {0}", songEntry.Name);
                    HandleDownload(songEntry, download, false);
                }
            }
        }

        /// <summary>
        ///     Use as callback of every downloads progress
        /// </summary>
        private void DownloadProgress(DownloadOperation download)
        {
            //Thread safety comes first!
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //Get the associated song BackgroundDownload
                var songDownload =
                    ActiveDownloads.FirstOrDefault(p => ((DownloadOperation) p.Download.DownloadOperation).Guid == download.Guid);

                if (songDownload == null)
                    return;

                Debug.WriteLine("Updating song BackgroundDownload progress for {0}", songDownload.Name);

                songDownload.Download.Status = download.Progress.Status.ToString();

                if (download.Progress.TotalBytesToReceive > 0)
                {
                    songDownload.Download.BytesToReceive = download.Progress.TotalBytesToReceive;
                    songDownload.Download.BytesReceived = download.Progress.BytesReceived;
                }
                else
                    songDownload.Download.Status = "Waiting";
            });
        }

        /// <summary>
        ///     Call internally to report a finished BackgroundDownload
        /// </summary>
        private async Task DownloadFinishedForAsync(Song song)
        {
            Debug.WriteLine("Download finished for {0}", song.Name);

            #region update id3 tags

            try
            {
                var file = ((DownloadOperation)song.Download.DownloadOperation).ResultFile;
                using (var fileStream = await file.OpenStreamForWriteAsync())
                {
                    var tagFile = File.Create(new SimpleFileAbstraction(file.Name, fileStream, fileStream));
                    var newTags = tagFile.GetTag(TagTypes.Id3v2, true);

                    newTags.Title = song.Name;

                    if (song.Artist.ProviderId != "autc.unknown")
                        newTags.Performers = song.ArtistName.Split(',').Select(p => p.Trim()).ToArray();

                    newTags.Album = song.Album.Name;
                    newTags.AlbumArtists = new[] {song.Album.PrimaryArtist.Name};

                    if (!string.IsNullOrEmpty(song.Album.Genre))
                        newTags.Genres = song.Album.Genre.Split(',').Select(p => p.Trim()).ToArray();

                    newTags.Track = (uint) song.TrackNumber;

                    newTags.Comment = "Downloaded with Audiotica - http://audiotica.fm";

                    if (song.Album.HasArtwork)
                    {
                        var albumFilePath = string.Format(CollectionConstant.ArtworkPath, song.Album.Id);
                        var artworkFile = await StorageHelper.GetFileAsync(albumFilePath);

                        using (var artworkStream = await artworkFile.OpenAsync(FileAccess.Read))
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
            }

            #endregion

            song.AudioUrl = ((DownloadOperation)song.Download.DownloadOperation).ResultFile.Path;
            song.SongState = SongState.Downloaded;
            song.DownloadId = null;
            await _sqlService.UpdateItemAsync(song);
        }

        /// <summary>
        ///     Hanbdles a single BackgroundDownload for a song.
        /// </summary>
        private async void HandleDownload(Song song, DownloadOperation download, bool start)
        {
            song.Download = new BackgroundDownload(download);
            ActiveDownloads.Add(song);
            Debug.WriteLine("Added {0} to active downloads", song.Name);

            var path = "Audiotica/" +
                       song.Album.PrimaryArtist.Name.CleanForFileName() + "/" +
                       song.Album.Name.CleanForFileName();
            var filename = string.Format("{0}.mp3", song.Name.CleanForFileName());

            if (song.ArtistName != song.Album.PrimaryArtist.Name)
                filename = song.ArtistName.CleanForFileName() + "-" + filename;
            try
            {
                var progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                if (start)
                {
                    // Start the BackgroundDownload and attach a progress handler.
                    await download.StartAsync().AsTask(song.Download.CancellationTokenSrc.Token, progressCallback);
                }
                else
                {
                    // The BackgroundDownload was already running when the application started, re-attach the progress handler.
                    await download.AttachAsync().AsTask(song.Download.CancellationTokenSrc.Token, progressCallback);
                }

                //Download Completed
                var response = download.GetResponseInformation();

                //Make sure it is success
                if (response.StatusCode < 400)
                    await DownloadFinishedForAsync(song);
                else
                {
                    Debug.WriteLine("Download status code for {0} is bad :/", song.Name);
                    song.SongState = SongState.None;
                    _sqlService.UpdateItem(song);
                    WinRtStorageHelper.DeleteFileAsync(path + filename, KnownFolders.MusicLibrary).Wait();
                }
            }
            catch
            {
                Debug.WriteLine("Download cancelled {0}", song.Name);

                song.SongState = SongState.None;
                _sqlService.UpdateItem(song);
                WinRtStorageHelper.DeleteFileAsync(path + filename, KnownFolders.MusicLibrary).Wait();
            }
            finally
            {
                ActiveDownloads.Remove(song);
            }
        }

        #endregion

        #region Implementation of ISongDownloadDataService

        public ObservableCollection<Song> ActiveDownloads { get; private set; }

        public async void LoadDownloads()
        {
            //Create empty collection for the song downloads
            ActiveDownloads = new ObservableCollection<Song>();
            await DiscoverActiveDownloadsAsync();

            Debug.WriteLine("Loaded downloads.");
        }

        public void PauseAll()
        {
            foreach (var activeDownload in ActiveDownloads)
            {
                ((DownloadOperation)activeDownload.Download.DownloadOperation).Pause();
            }
        }

        public void Cancel(BackgroundDownload backgroundDownload)
        {
            backgroundDownload.CancellationTokenSrc.Cancel();
        }

        public async Task StartDownloadAsync(Song song)
        {
            song.SongState = SongState.Downloading;

            try
            {
                var filename = song.Name.CleanForFileName();
                if (song.ArtistName != song.Album.PrimaryArtist.Name)
                    filename = song.ArtistName.CleanForFileName() + "-" + filename;
                var path = string.Format(CollectionConstant.SongPath, song.Album.PrimaryArtist.Name.CleanForFileName(), song.Album.Name.CleanForFileName(), filename);
                
                var destinationFile =
                    await
                        WinRtStorageHelper.CreateFileAsync(path, KnownFolders.MusicLibrary,
                            CreationCollisionOption.ReplaceExisting)
                            .ConfigureAwait(false);

                var downloader = new BackgroundDownloader();
                var download = downloader.CreateDownload(new Uri(song.AudioUrl), destinationFile);
                download.Priority = BackgroundTransferPriority.Default;
                song.DownloadId = download.Guid.ToString();

                await _sqlService.UpdateItemAsync(song).ConfigureAwait(false);
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HandleDownload(song, download, true));
            }
            catch (Exception e)
            {
                if (e.Message.Contains("there is not enough space on the disk"))
                    _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () => CurtainPrompt.ShowError("Not enough disk space to download."));

                _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    song.SongState = SongState.None);
                _sqlService.UpdateItemAsync(song).ConfigureAwait(false);
            }
        }

        #endregion
    }
}