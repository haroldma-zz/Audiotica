using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Windows.Storage;
using Audiotica.Core.Utils;

namespace Audiotica.Core.WinRt.Utilities
{
    public static class WinRtStorageHelper
    {
        private static StorageFolder GetFolderFromStrategy(StorageHelper.StorageStrategy location)
        {
            switch (location)
            {
                case StorageHelper.StorageStrategy.Roaming:
                    return ApplicationData.Current.RoamingFolder;
                default:
                    return ApplicationData.Current.LocalFolder;
            }
        }

        public static async Task<StorageFile> GetFileAsync(string path,
           StorageHelper.StorageStrategy location = StorageHelper.StorageStrategy.Local)
        {
            return await CreateFileAsync(path, GetFolderFromStrategy(location));
        }

        public static async Task<StorageFile> GetFileAsync(string path,
            StorageFolder folder)
        {
            return await CreateFileAsync(path, folder);
        }

        public static async Task<StorageFile> CreateFileAsync(string path, StorageFolder folder,
          CreationCollisionOption option = CreationCollisionOption.OpenIfExists)
        {
            if (path.StartsWith("/") || path.StartsWith("\\"))
                path = path.Substring(1);
            var parts = path.Split('/');

            var fileName = parts.Last();

            if (parts.Length > 1)
            {
                folder =
                    await
                        EnsureFolderExistsAsync(path.Substring(0, path.Length - fileName.Length), folder)
                            .ConfigureAwait(false);
            }

            return await folder.CreateFileAsync(fileName, option).AsTask().ConfigureAwait(false);
        }

        public static async Task<StorageFolder> GetFolderAsync(string path,
           StorageHelper.StorageStrategy location = StorageHelper.StorageStrategy.Local)
        {
            return await GetFolderAsync(path, GetFolderFromStrategy(location)).ConfigureAwait(false);
        }

        public static async Task<StorageFolder> GetFolderAsync(string path, StorageFolder parentFolder)
        {
            var parent = parentFolder;

            foreach (var name in path.Trim('/').Split('/'))
            {
                parent = await _GetFolderAsync(name, parent).ConfigureAwait(false);

                if (parent == null) return null;
            }

            return parent; // now points to innermost folder
        }

        private static async Task<StorageFolder> _GetFolderAsync(string name, StorageFolder parent)
        {
            var folders = await parent.GetFoldersAsync().AsTask().ConfigureAwait(false);
            var item = folders.FirstOrDefault(p => p.Name == name);
            return item;
        }

        public static async Task<StorageFolder> EnsureFolderExistsAsync(string path, StorageFolder parentFolder)
        {
            var parent = parentFolder;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var name in path.Trim('/').Split('/'))
            {
                parent = await _EnsureFolderExistsAsync(name, parent).ConfigureAwait(false);
            }

            return parent; // now points to innermost folder
        }

        private static async Task<StorageFolder> _EnsureFolderExistsAsync(string name, StorageFolder parent)
        {
            if (parent == null) throw new ArgumentNullException("parent");
            return
                await
                    parent.CreateFolderAsync(name, CreationCollisionOption.OpenIfExists).AsTask().ConfigureAwait(false);
        }

        public static async Task DeleteFileAsync(string path, StorageFolder folder)
        {
            var file = await GetIfFileExistsAsync(path, folder).ConfigureAwait(false);

            if (file != null)
                await file.DeleteAsync();
        }

        private static async Task<StorageFile> GetIfFileExistsAsync(string path, StorageFolder folder)
        {
            var parts = path.Split('/');

            var fileName = parts.Last();

            if (parts.Length > 1)
            {
                folder =
                    await GetFolderAsync(path.Substring(0, path.Length - fileName.Length), folder).ConfigureAwait(false);
            }

            if (folder == null)
            {
                return null;
            }
            var files = await folder.GetFilesAsync().AsTask().ConfigureAwait(false);
            return files.FirstOrDefault(p => p.Name == fileName);
        }
    }
}