﻿#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Core;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Service.Interfaces;

#endregion

namespace Audiotica.Data.Service.RunTime
{
    public class SongDownloadService : ISongDownloadService
    {
        #region Private Fields

        private readonly CoreDispatcher _dispatcher;
        private readonly ICollectionService _service;
        private readonly ISqlService _sqlService;

        #endregion

        #region Constructor

        public SongDownloadService(ICollectionService service, ISqlService sqlService, CoreDispatcher dispatcher)
        {
            _service = service;
            _sqlService = sqlService;
            _dispatcher = dispatcher;
        }

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
                //Get id from the results file
                var id = int.Parse(download.ResultFile.Name.Substring(0,
                    download.ResultFile.Name.IndexOf(".", StringComparison.Ordinal)));

                //With that id get the song
                var songEntry = _service.Songs.First(p => p.Id == id);

                Debug.WriteLine("Handling downoad for {0}", songEntry.Name);
                HandleDownload(songEntry, download, false);
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
                    ActiveDownloads.FirstOrDefault(p => p.Download.DownloadOperation.Guid == download.Guid);

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

            //Update the IsDownloading property
            song.SongState = SongState.Downloaded;
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

            var path = "Audiotica/" + song.Album.PrimaryArtist.Name + "/" + song.Album.Name;
            var filename = string.Format("{0}.mp3", song.Name);
            if (song.ArtistName != song.Album.PrimaryArtist.Name)
                filename = song.ArtistName + "-" + filename;

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
                    StorageHelper.DeleteFileAsync(path + filename, KnownFolders.MusicLibrary).Wait();
                }
            }
            catch
            {
                Debug.WriteLine("Download cancelled {0}", song.Name);

                song.SongState = SongState.None;
                _sqlService.UpdateItem(song);
                StorageHelper.DeleteFileAsync(path + filename, KnownFolders.MusicLibrary).Wait();
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
                activeDownload.Download.DownloadOperation.Pause();
            }
        }

        public void Cancel(BackgroundDownload backgroundDownload)
        {
            backgroundDownload.CancellationTokenSrc.Cancel();
        }

        public async Task StartDownloadAsync(Song song)
        {
            song.SongState = SongState.Downloading;

            await _sqlService.UpdateItemAsync(song).ConfigureAwait(false);
            try
            {
                var path = "Audiotica/" + song.Album.PrimaryArtist.Name + "/" + song.Album.Name + "/";
                var filename = string.Format("{0}.mp3", song.Name);
                if (song.ArtistName != song.Album.PrimaryArtist.Name)
                    filename = song.ArtistName + "-" + filename;

                if (await StorageHelper.FileExistsAsync(path + "/" + filename, KnownFolders.MusicLibrary))
                {
                    _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => song.SongState = SongState.None);
                    _sqlService.UpdateItemAsync(song).ConfigureAwait(false);
                    return;
                }

                var destinationFile =
                    await
                        StorageHelper.CreateFileAsync(path + "/" + filename, KnownFolders.MusicLibrary)
                            .ConfigureAwait(false);

                var downloader = new BackgroundDownloader();
                var download = downloader.CreateDownload(new Uri(song.AudioUrl), destinationFile);
                download.Priority = BackgroundTransferPriority.Default;

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