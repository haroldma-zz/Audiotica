using System.IO;
using Windows.Storage;
using Audiotica.Core.Helpers;
using Audiotica.Core.Windows.Helpers;
using Autofac;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;

namespace Audiotica.Windows.AppEngine.Providers
{
    // ReSharper disable once InconsistentNaming
    internal class SQLiteConnectionProvider : IProvider<SQLiteConnection>
    {
        private const string DatabaseFilename = @"Library\audiotica.sqlite";

        public SQLiteConnection CreateInstance(IComponentContext context)
        {
            AsyncHelper.RunSync(() => StorageHelper.EnsureFolderExistsAsync("Library"));
            var path = Path.Combine(ApplicationData.Current.LocalFolder.Path, DatabaseFilename);
            return new SQLiteConnection(new SQLitePlatformWinRT(), path);
        }
    }
}