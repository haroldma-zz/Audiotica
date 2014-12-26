using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Audiotica.Data.Collection.Model;
using GalaSoft.MvvmLight;

namespace Audiotica.Data.Service.Interfaces
{
    public interface ISongDownloadService
    {
        ObservableCollection<SongDownload> ActiveDownloads { get; }

        /// <summary>
        ///     Loads all downloads and attaches to them
        /// </summary>
        /// <returns></returns>
        void LoadDownloads();

        /// <summary>
        ///     Pause all downloads
        /// </summary>
        void PauseAll();

        /// <summary>
        ///     Cancles the download
        /// </summary>
        void Cancel(SongDownload download);

        /// <summary>
        ///     Starts a download.
        /// </summary>
        void StartDownload(Song song);
    }

    public class SongDownload : ObservableObject
    {
        #region Private Fields

        private double _bytesReceived;
        private double _bytesToReceive;
        private Song _song;
        private string _status = "Waiting";

        #endregion

        #region Constructor

        public SongDownload(Song song, DownloadOperation downloadOperation)
        {
            _song = song;
            DownloadOperation = downloadOperation;
        }

        #endregion

        #region Public Properties

        public double BytesToReceive
        {
            get { return _bytesToReceive; }
            set { Set(ref _bytesToReceive, value); }
        }

        public double BytesReceived
        {
            get { return _bytesReceived; }
            set { Set(ref _bytesReceived, value); }
        }

        public Song Song
        {
            get { return _song; }
            set { Set(ref _song, value); }
        }

        public string Status
        {
            get { return _status; }
            set { Set(ref _status, value); }
        }

        public DownloadOperation DownloadOperation { get; private set; }

        #endregion
    }
}
