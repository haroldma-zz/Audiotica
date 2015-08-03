using System.IO;
using System.Threading.Tasks;
using Audiotica.Core.Helpers;
using Audiotica.Core.Utilities.Interfaces;

namespace Audiotica.Core.Utilities.DesignTime
{
    public class DesignStorageUtility : IStorageUtility
    {
        public Task<byte[]> ReadBytesAsync(string path, bool ifExists = false,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            return Task.FromResult<byte[]>(null);
        }

        public Task<string> ReadStringAsync(string path, bool ifExists = false,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            return Task.FromResult<string>(null);
        }

        public Task<Stream> ReadStreamAsync(string path, bool ifExists = false,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            return Task.FromResult<Stream>(null);
        }

        public Task WriteBytesAsync(string path, byte[] bytes,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            return Task.FromResult(0);
        }

        public Task WriteStringAsync(string path, string text,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            return Task.FromResult(0);
        }

        public Task WriteStreamAsync(string path, Stream stream,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            return Task.FromResult(0);
        }

        public Task<bool> DeleteAsync(string path,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            return Task.FromResult(false);
        }

        public Task<bool> ExistsAsync(string path,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            return Task.FromResult(false);
        }
    }
}