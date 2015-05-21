using System.IO;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Helpers;
using Audiotica.Core.Interfaces.Utilities;

namespace Audiotica.Core.Utilities
{
    /// <summary>
    /// A wrapper for AppStorageHelper that simplifies it and makes it DI compatible (interface support).
    /// </summary>
    public class StorageUtility : IStorageUtility
    {
        public async Task<Stream> ReadStreamAsync(string path, bool ifExists = false,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            var file = await (ifExists ? PclStorageHelper.GetIfFileExistsAsync(path, location) : PclStorageHelper.GetFileAsync(path, location));
            if (file == null) return null;
            return await file.OpenStreamForReadAsync();
        }

        public async Task<byte[]> ReadBytesAsync(string path, bool ifExists = false,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            var file = await (ifExists ? PclStorageHelper.GetIfFileExistsAsync(path, location) : PclStorageHelper.GetFileAsync(path, location));
            if (file == null) return null;
            using (var stream = await file.OpenStreamForReadAsync())
            {
                return await stream.ToByteArrayAsync();
            }
        }

        public async Task WriteStreamAsync(string path, Stream stream,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            var file = await PclStorageHelper.GetFileAsync(path, location);
            using (var fileStream = await file.OpenStreamForWriteAsync())
            {
                if (stream.Position > 0)
                    stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(fileStream);
            }
        }

        public async Task WriteBytesAsync(string path, byte[] bytes,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            var file = await PclStorageHelper.GetFileAsync(path, location);
            using (var fileStream = await file.OpenStreamForWriteAsync())
            {
                await fileStream.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        public async Task<bool> DeleteAsync(string path,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            return await PclStorageHelper.DeleteFileAsync(path, location);
        }

        public async Task<bool> ExistsAsync(string path,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            return await PclStorageHelper.FileExistsAsync(path, location);
        }
    }
}