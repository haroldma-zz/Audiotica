using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace Audiotica.Windows.Services.Interfaces
{
    public interface IMusicImportService
    {
        Task SaveAsync(StorageFile file);
        Task<List<StorageFile>> ScanFolderAsync(IStorageFolder folder);
    }
}