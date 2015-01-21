using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Audiotica.Data.Collection.Model;

namespace Audiotica.Data.Collection
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
