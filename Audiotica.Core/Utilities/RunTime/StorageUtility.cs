using System.IO;
using System.Text;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Helpers;
using Audiotica.Core.Utilities.Interfaces;

namespace Audiotica.Core.Utilities.RunTime
{
    /// <summary>
    ///     A wrapper for PclStorageHelper that simplifies it and makes it DI compatible (interface support).
    /// </summary>
    public class StorageUtility : IStorageUtility
    {
        public async Task<Stream> ReadStreamAsync(string path, bool ifExists = false,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            var file =
                await
                    (ifExists
                        ? PclStorageHelper.GetIfFileExistsAsync(path, location)
                        : PclStorageHelper.GetFileAsync(path, location)).DontMarshall();
            if (file == null) return null;
            return await file.OpenStreamForReadAsync().DontMarshall();
        }

        public async Task<byte[]> ReadBytesAsync(string path, bool ifExists = false,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            var file =
                await
                    (ifExists
                        ? PclStorageHelper.GetIfFileExistsAsync(path, location)
                        : PclStorageHelper.GetFileAsync(path, location)).DontMarshall();
            if (file == null) return null;
            using (var stream = await file.OpenStreamForReadAsync().DontMarshall())
            {
                var bytes = new byte[stream.Length];
                await stream.ReadAsync(bytes, 0, bytes.Length).DontMarshall();
                return bytes;
            }
        }

        public async Task WriteStreamAsync(string path, Stream stream,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            var file = await PclStorageHelper.GetFileAsync(path, location).DontMarshall();
            using (var fileStream = await file.OpenStreamForWriteAsync().DontMarshall())
            {
                if (stream.Position > 0)
                    stream.Seek(0, SeekOrigin.Begin);
                if (fileStream.Length > 0)
                    fileStream.SetLength(0);
                await stream.CopyToAsync(fileStream).DontMarshall();
            }
        }

        public async Task WriteBytesAsync(string path, byte[] bytes,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            var file = await PclStorageHelper.GetFileAsync(path, location).DontMarshall();
            using (var fileStream = await file.OpenStreamForWriteAsync().DontMarshall())
            {
                if (fileStream.Length > 0)
                    fileStream.SetLength(0);
                await fileStream.WriteAsync(bytes, 0, bytes.Length).DontMarshall();
            }
        }

        public async Task<bool> DeleteAsync(string path,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            return await PclStorageHelper.DeleteFileAsync(path, location).DontMarshall();
        }

        public async Task<bool> ExistsAsync(string path,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            return await PclStorageHelper.FileExistsAsync(path, location).DontMarshall();
        }

        public async Task<string> ReadStringAsync(string path, bool ifExists = false,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            var bytes = await ReadBytesAsync(path, ifExists, location).DontMarshall();
            return bytes == null ? null : Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public async Task WriteStringAsync(string path, string text,
            PclStorageHelper.StorageStrategy location = PclStorageHelper.StorageStrategy.Local)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            await WriteBytesAsync(path, bytes, location).DontMarshall();
        }
    }
}