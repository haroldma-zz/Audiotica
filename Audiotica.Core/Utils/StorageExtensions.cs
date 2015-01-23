#region

using System.Linq;
using System.Threading.Tasks;
using PCLStorage;

#endregion

namespace Audiotica.Core.Utils
{
    public static class StorageExtensions
    {
        public static async Task<IFile> TryGetFileAsync(this IFolder folder,
            string name)
        {
            var files = (await folder.GetFilesAsync().ConfigureAwait(false)).ToList();
            return files.FirstOrDefault(p => p.Name == name);
        }

        public static async Task<IFolder> TryGetFolderAsync(this IFolder folder,
           string name)
        {
            var folders = (await folder.GetFoldersAsync().ConfigureAwait(false)).ToList();
            return folders.FirstOrDefault(p => p.Name == name);
        }
    }
}