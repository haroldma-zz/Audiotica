using System.IO;
using System.Threading.Tasks;
using Audiotica.Core.Helpers;

namespace Audiotica.Core.Interfaces.Utilities
{
    public interface IStorageUtility
    {
        Task<byte[]> ReadBytesAsync(string path, bool ifExists = false,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local);

        Task<Stream> ReadStreamAsync(string path, bool ifExists = false,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local);

        Task WriteBytesAsync(string path, byte[] bytes,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local);

        Task WriteStreamAsync(string path, Stream stream,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local);

        Task<bool> DeleteAsync(string path,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local);

        Task<bool> ExistsAsync(string path,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local);
    }
}