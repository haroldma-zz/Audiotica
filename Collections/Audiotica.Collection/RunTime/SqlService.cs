using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Audiotica.Collection.Model;
using Audiotica.Core.Utilities;
using SQLite;

namespace Audiotica.Collection.RunTime
{
    public class SqlService : ISqlService
    {
        #region Private Fields

        private const string DbName = "Audiotica.sqlite";
        private SQLiteConnection _db;
        private const double DatabaseVersion = 0.1;

        #endregion

        public SQLiteConnection Connection { get { return _db; } }

        public void Initialize()
        {
            _db = new SQLiteConnection(DbName);
            Debug.WriteLine("Created SQL connection");

            var currentVersion = AppSettingsHelper.Read<double>("DatabaseVersion");

            if (currentVersion.Equals(0.0))
            {
                CreateTables();
                AppSettingsHelper.Write("DatabaseVersion", DatabaseVersion);
            }
            else if (currentVersion < DatabaseVersion)
            {
                //In the future do changes here
            }
            Debug.WriteLine("Database initialize successfuly!");
        }

        private void CreateTables()
        {
            //Contains all the songs
            _db.CreateTable<Song>();

            //Contains all the artists
            _db.CreateTable<Artist>();

            //Contains all the albums
            _db.CreateTable<Album>();

            //use for now playing queue
            _db.CreateTable<QueueSong>();

            //use for playlists
            _db.CreateTable<PlaylistSong>();

            Debug.WriteLine("Created tables");
        }

        public Task InitializeAsync()
        {
            return Task.Factory.StartNew(Initialize);
        }

        #region Sql async wrapper

        public Task<List<T>> GetAllAsync<T>() where T : new()
        {
            return Task.FromResult(_db.Table<T>().ToList());
        }

        public Task<List<T>> GetWhereAsync<T>(Func<T, bool> predicate) where T : new()
        {
            return Task.FromResult(_db.Table<T>().Where(predicate).ToList());
        }

        public Task InsertAsync<T>(T obj)
        {
            return Task.FromResult(_db.Insert(obj));
        }

        public Task UpdateAsync<T>(T obj)
        {
            return Task.FromResult(_db.Update(obj));
        }

        public Task DeleteAsync<T>(T obj)
        {
            return Task.FromResult(_db.Delete(obj));
        }

        public Task DeleteAllAsync<T>()
        {
            return Task.FromResult(_db.DeleteAll<T>());
        }

        public Task<T> GetFirstAsync<T>(Func<T, bool> predicate) where T : new()
        {
            return Task.FromResult(_db.Table<T>().FirstOrDefault(predicate));
        }

        #endregion
    }
}
