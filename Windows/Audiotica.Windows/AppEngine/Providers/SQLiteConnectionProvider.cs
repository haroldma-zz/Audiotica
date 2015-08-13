using Autofac;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;

namespace Audiotica.Windows.AppEngine.Providers
{
    // ReSharper disable once InconsistentNaming
    internal class SQLiteConnectionProvider : IProvider<SQLiteConnection>
    {
        private const string DatabaseFilename = "audiotica.sql";

        public SQLiteConnection CreateInstance(IComponentContext context)
        {
            return new SQLiteConnection(new SQLitePlatformWinRT(), DatabaseFilename);
        }
    }
}