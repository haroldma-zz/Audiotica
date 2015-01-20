#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Certificates;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.SqlHelper;
using SQLite;

#endregion

namespace Audiotica.Data.Collection.RunTime
{
    public class SqlService : ISqlService
    {
        private readonly SqlServiceConfig _config;

        public SqlService(SqlServiceConfig config)
        {
            _config = config;
        }

        public SQLiteConnection DbConnection { get; set; }

        public void Dispose()
        {
            DbConnection.Dispose();
            GC.Collect();
        }

        public void Initialize(bool walMode = true)
        {
            DbConnection = new SQLiteConnection(_config.Path);
            var sqlVersion = DbConnection.ExecuteScalar<int>("PRAGMA user_version");
            if (sqlVersion == _config.CurrentVersion) return;

            //using wal so the player and app can access the db without worrying about it being locked
            DbConnection.ExecuteScalar<string>("PRAGMA journal_mode = " + (walMode ? "WAL" : "DELETE"));

            if (_config.OnUpdate != null)
                _config.OnUpdate(DbConnection, sqlVersion);

            /*
             * Callback function is invoked once for each DELETE, INSERT, or UPDATE operation. 
             * The argument is the number of rows that were changed
             * Turning this off will give a small speed boost 
             */
            DbConnection.ExecuteScalar<string>("PRAGMA count_changes = OFF");

            //Data integrity is not a top priority, performance is.
            DbConnection.ExecuteScalar<string>("PRAGMA synchronous = OFF");

            DbConnection.ExecuteScalar<string>("PRAGMA foreign_keys = ON");

            CreateTablesIfNotExists();
        }

        public async Task InitializeAsync()
        {
            await Task.Run(() => Initialize()).ConfigureAwait(false);
        }

        public bool Insert(BaseEntry entry)
        {
            try
            {
                DbConnection.Insert(entry);
                return true;
            }
            catch (SQLiteException e)
            {
                return e.Result == SQLite3.Result.Busy && Insert(entry);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> InsertAsync(BaseEntry entry)
        {
            return await Task.FromResult(Insert(entry)).ConfigureAwait(false);
        }

        public bool DeleteItem(BaseEntry item)
        {
            try
            {
                DbConnection.Delete(item);
                return true;
            }
            catch (SQLiteException e)
            {
                return e.Result == SQLite3.Result.Busy && DeleteItem(item);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteItemAsync(BaseEntry item)
        {
            return await Task.FromResult(DeleteItem(item)).ConfigureAwait(false);
        }

        public bool UpdateItem(BaseEntry item)
        {
            try
            {
                DbConnection.Update(item);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateItemAsync(BaseEntry item)
        {
            return await Task.FromResult(UpdateItem(item)).ConfigureAwait(false);
        }

        public async Task<List<T>> SelectAllAsync<T>() where T : new()
        {
            return await Task.Factory.StartNew(() => SelectAll<T>()).ConfigureAwait(false);
        }

        public List<T> SelectAll<T>() where T : new()
        {
            try
            {
                return DbConnection.Table<T>().ToList();
            }
            catch (SQLiteException e)
            {
                if (e.Result == SQLite3.Result.Busy)
                    return SelectAll<T>();
            }
            catch
            {
            }
            return new List<T>();
        }

        public T SelectFirst<T>(Func<T, bool> expression) where T : new()
        {
            try
            {
                return DbConnection.Table<T>().FirstOrDefault(expression);
            }
            catch (SQLiteException e)
            {
                if (e.Result == SQLite3.Result.Busy)
                    return SelectFirst(expression);
            }
            catch
            {
            }
            return default(T);
        }

        public Task DeleteTableAsync<T>()
        {
            return Task.Run(() =>
            {
                DbConnection.DeleteAll<T>();
                DbConnection.Execute("DELETE FROM sqlite_sequence  WHERE name = '" + typeof(T).Name + "'");
            });
        }

        public Task DeleteWhereAsync(BaseEntry entry)
        {
            return Task.Run(() =>
            {
                DbConnection.Delete(entry);
            });
        }

        private void CreateTablesIfNotExists()
        {
            foreach (var type in _config.Tables)
            {
                DbConnection.CreateTable(type);
            }

            UpdateDbVersion(_config.CurrentVersion);
        }

        private void UpdateDbVersion(double version)
        {
            DbConnection.Execute("PRAGMA user_version = " + version);
        }
    }

    public class SqlServiceConfig
    {
        public Action<SQLiteConnection, double> OnUpdate;
        public double CurrentVersion { get; set; }
        public string Path { get; set; }
        public List<Type> Tables { get; set; }
    }
}