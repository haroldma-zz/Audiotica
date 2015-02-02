#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using SQLite;

#endregion

namespace Audiotica.Data.Collection.RunTime
{
    public class SqlService : ISqlService
    {
        private readonly SqlServiceConfig _config;

        private bool _isInit;

        public SqlService(SqlServiceConfig config)
        {
            this._config = config;
        }

        public SQLiteConnection DbConnection { get; set; }

        public void Dispose()
        {
            this.DbConnection.Dispose();
            this._isInit = false;
            GC.Collect();
        }

        public void Initialize(bool walMode = true, bool readOnlyMode = false)
        {
            if (this._isInit)
            {
                return;
            }

            var flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create;
            if (readOnlyMode)
            {
                flags = SQLiteOpenFlags.ReadOnly;
            }

            this.DbConnection = new SQLiteConnection(this._config.Path, flags);

            // using wal so the player and app can access the db without worrying about it being locked
            this.DbConnection.ExecuteScalar<string>("PRAGMA journal_mode = " + (walMode ? "WAL" : "DELETE"));

            var sqlVersion = this.DbConnection.ExecuteScalar<int>("PRAGMA user_version");

            this._isInit = true;

            if (sqlVersion == this._config.CurrentVersion)
            {
                return;
            }

            if (this._config.OnUpdate != null)
            {
                try
                {
                    this._config.OnUpdate(this.DbConnection, sqlVersion);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message);
                }
            }

            /*
             * Callback function is invoked once for each DELETE, INSERT, or UPDATE operation. 
             * The argument is the number of rows that were changed
             * Turning this off will give a small speed boost 
             */
            this.DbConnection.ExecuteScalar<string>("PRAGMA count_changes = OFF");

            // Data integrity is not a top priority, performance is.
            this.DbConnection.ExecuteScalar<string>("PRAGMA synchronous = OFF");

            this.DbConnection.ExecuteScalar<string>("PRAGMA foreign_keys = ON");

            this.CreateTablesIfNotExists();
        }

        public async Task InitializeAsync()
        {
            await Task.Run(() => this.Initialize()).ConfigureAwait(false);
        }

        public bool Insert(BaseEntry entry)
        {
            try
            {
                this.DbConnection.Insert(entry);
                return true;
            }
            catch (SQLiteException e)
            {
                return e.Result == SQLite3.Result.Busy && this.Insert(entry);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> InsertAsync(BaseEntry entry)
        {
            return await Task.FromResult(this.Insert(entry)).ConfigureAwait(false);
        }

        public bool DeleteItem(BaseEntry item)
        {
            try
            {
                this.DbConnection.Delete(item);
                return true;
            }
            catch (SQLiteException e)
            {
                return e.Result == SQLite3.Result.Busy && this.DeleteItem(item);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteItemAsync(BaseEntry item)
        {
            return await Task.FromResult(this.DeleteItem(item)).ConfigureAwait(false);
        }

        public bool UpdateItem(BaseEntry item)
        {
            try
            {
                this.DbConnection.Update(item);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateItemAsync(BaseEntry item)
        {
            return await Task.FromResult(this.UpdateItem(item)).ConfigureAwait(false);
        }

        public async Task<List<T>> SelectAllAsync<T>() where T : new()
        {
            return await Task.Factory.StartNew(() => this.SelectAll<T>()).ConfigureAwait(false);
        }

        public List<T> SelectAll<T>() where T : new()
        {
            try
            {
                return this.DbConnection.Table<T>().ToList();
            }
            catch (SQLiteException e)
            {
                if (e.Result == SQLite3.Result.Busy)
                {
                    return this.SelectAll<T>();
                }
            }
            catch
            {
                // ignored
            }

            return new List<T>();
        }

        public Task<T> SelectFirstAsync<T>(Func<T, bool> expression) where T : new()
        {
            return Task.FromResult(this.SelectFirst(expression));
        }

        public T SelectFirst<T>(Func<T, bool> expression) where T : new()
        {
            try
            {
                return this.DbConnection.Table<T>().FirstOrDefault(expression);
            }
            catch (SQLiteException e)
            {
                if (e.Result == SQLite3.Result.Busy)
                {
                    return this.SelectFirst(expression);
                }
            }
            catch
            {
                // ignored
            }

            return default(T);
        }

        public Task DeleteTableAsync<T>()
        {
            return Task.Run(
                () =>
                {
                    this.DbConnection.DeleteAll<T>();
                    this.DbConnection.Execute("DELETE FROM sqlite_sequence  WHERE name = '" + typeof(T).Name + "'");
                });
        }

        public Task DeleteWhereAsync(BaseEntry entry)
        {
            return Task.Run(() => { this.DbConnection.Delete(entry); });
        }

        public void BeginTransaction()
        {
            try
            {
                this.DbConnection.BeginTransaction();
            }
            catch (InvalidOperationException)
            {
            }
        }

        public void Commit()
        {
            try
            {
                this.DbConnection.Commit();
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void CreateTablesIfNotExists()
        {
            foreach (var type in this._config.Tables)
            {
                this.DbConnection.CreateTable(type);
            }

            this.UpdateDbVersion(this._config.CurrentVersion);
        }

        private void UpdateDbVersion(double version)
        {
            this.DbConnection.Execute("PRAGMA user_version = " + version);
        }
    }

    public class SqlServiceConfig
    {
        public Action<SQLiteConnection, double> OnUpdate;

        public int CurrentVersion { get; set; }

        public string Path { get; set; }

        public List<Type> Tables { get; set; }
    }
}