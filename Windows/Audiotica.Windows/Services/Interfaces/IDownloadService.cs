using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Audiotica.Database.Models;

namespace Audiotica.Windows.Services.Interfaces
{
    public interface IDownloadService
    {
        ObservableCollection<Track> ActiveDownloads { get; }
        Task StartDownloadAsync(Track track);
        void Cancel(BackgroundDownload backgroundDownload);
        void PauseAll();
        void LoadDownloads();
    }
}