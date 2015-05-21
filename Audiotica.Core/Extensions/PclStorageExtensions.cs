using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PCLStorage;

namespace Audiotica.Core.Extensions
{
    public static class PclStorageExtensions
    {
        public static async Task<IFile> TryGetFileAsync(this IFolder folder,
            string name)
        {
            var files = (await folder.GetFilesAsync().DontMarshall()).ToList();
            return files.FirstOrDefault(p => p.Name == name);
        }

        public static async Task<IFolder> TryGetFolderAsync(this IFolder folder,
            string name)
        {
            var folders = (await folder.GetFoldersAsync().DontMarshall()).ToList();
            return folders.FirstOrDefault(p => p.Name == name);
        }

        public static Task<Stream> OpenStreamForReadAsync(this IFile file)
        {
            return file.OpenAsync(FileAccess.Read);
        }

        public static Task<Stream> OpenStreamForWriteAsync(this IFile file)
        {
            return file.OpenAsync(FileAccess.ReadAndWrite);
        }

        public static async Task<byte[]> ToByteArrayAsync(this Stream stream)
        {
            var bytes = new byte[stream.Length];
            await stream.ReadAsync(bytes, 0, bytes.Length);
            return bytes;
        }
    }
}