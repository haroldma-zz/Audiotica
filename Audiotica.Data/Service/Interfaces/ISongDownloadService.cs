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
        ObservableCollection<Song> ActiveDownloads { get; }

        /// <summary>
        ///     Loads all downloads and attaches to them
        /// </summary>
        void LoadDownloads();

        /// <summary>
        ///     Pause all downloads
        /// </summary>
        void PauseAll();

        /// <summary>
        ///     Cancles the BackgroundDownload
        /// </summary>
        void Cancel(BackgroundDownload backgroundDownload);

        /// <summary>
        ///     Starts a BackgroundDownload.
        /// </summary>
        Task StartDownloadAsync(Song song);
    }
}
