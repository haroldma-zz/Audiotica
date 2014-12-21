#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.SqlHelper;
using SQLitePCL;

#endregion

namespace Audiotica.Data.Collection.RunTime
{
    public class SqlService : ISqlService, IDisposable
    {
        private const long CurrentDbVersion = 2;

        public SqlService()
        {
            DbConnection = new SQLiteConnection("collection.sqldb");
        }

        public SQLiteConnection DbConnection { get; set; }

        public void Dispose()
        {
            DbConnection.Dispose();
        }

        public void Initialize()
        {
            long sqlVersion;

            var sql = "PRAGMA user_version";
            using (var statement = DbConnection.Prepare(sql))
            {
                statement.Step();
                sqlVersion = (long) statement[0];
            }

            bool dbOldCreated;
            sql = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='Song'";
            using (var statement = DbConnection.Prepare(sql))
            {
                statement.Step();
                dbOldCreated = (long) statement[0] != 0 && sqlVersion == 0;
            }

            if (sqlVersion == CurrentDbVersion) return;

            if (dbOldCreated)
            {
                //Update db from Beta5 (Patch #1) and down
                sql = "ALTER TABLE Song ADD COLUMN LastPlayed DATETIME";
                using (var statement = DbConnection.Prepare(sql))
                {
                    statement.Step();
                }

                sql = "ALTER TABLE Song ADD COLUMN Duration BIGINT";
                using (var statement = DbConnection.Prepare(sql))
                {
                    statement.Step();
                }
            }

            CreateTablesIfNotExists();
        }

        public Task InitializeAsync()
        {
            return Task.Factory.StartNew(Initialize);
        }

        public void ResetData()
        {
            var sql = @"DELETE FROM Song";
            using (var statement = DbConnection.Prepare(sql))
            {
                statement.Step();
            }

            sql = @"DELETE FROM Album";
            using (var statement = DbConnection.Prepare(sql))
            {
                statement.Step();
            }

            sql = @"DELETE FROM Artist";
            using (var statement = DbConnection.Prepare(sql))
            {
                statement.Step();
            }

            sql = @"DELETE FROM QueueSong";
            using (var statement = DbConnection.Prepare(sql))
            {
                statement.Step();
            }

            sql = @"DELETE FROM PlaylistSong";
            using (var statement = DbConnection.Prepare(sql))
            {
                statement.Step();
            }
        }


        public async Task InsertAsync(BaseEntry entry)
        {
            try
            {
                await Task.Run(() =>
                {
                    using (var custstmt = DbConnection.Prepare(EasySql.CreateInsert(entry.GetType())))
                    {
                        EasySql.FillInsert(custstmt, entry);
                        var res = custstmt.Step();
                        if (res != SQLiteResult.DONE)
                            throw new Exception();
                    }
                }
                    );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return;
            }

            using (var idstmt = DbConnection.Prepare("SELECT last_insert_rowid()"))
            {
                idstmt.Step();
                {
                    entry.Id = (long) idstmt[0];
                }
            }
        }

        public Task DeleteItemAsync(BaseEntry item)
        {
            return Task.Run(() =>
            {
                using (
                    var projstmt =
                        DbConnection.Prepare(string.Format("DELETE FROM {0} WHERE Id = ?", item.GetType().Name)))
                {
                    // Reset the prepared statement so we can reuse it.
                    projstmt.ClearBindings();
                    projstmt.Reset();

                    projstmt.Bind(1, item.Id);

                    projstmt.Step();
                }
            });
        }

        public Task UpdateItemAsync(BaseEntry item)
        {
            return Task.Run(() =>
            {
                using (
                    var projstmt =
                        DbConnection.Prepare(EasySql.CreateUpdate(item.GetType())))
                {
                    // Reset the prepared statement so we can reuse it.
                    projstmt.ClearBindings();
                    projstmt.Reset();

                    EasySql.FillUpdate(projstmt, item);

                    projstmt.Step();
                }
            });
        }

        public List<T> SelectAll<T>() where T : new()
        {
            var type = typeof (T);
            var items = new List<T>();

            using (var statement = DbConnection.Prepare("SELECT * FROM " + type.Name))
            {
                while (statement.Step() == SQLiteResult.ROW)
                {
                    var item = new T();
                    var props =
                        type.GetRuntimeProperties()
                            .Where(
                                p =>
                                    p.GetCustomAttribute<SqlIgnore>() == null &&
                                    EasySql.NetToSqlKepMap.ContainsKey(p.PropertyType));

                    foreach (var propertyInfo in props)
                    {
                        var value = statement[propertyInfo.Name];

                        //cast enums from long
                        if (propertyInfo.GetMethod.ReturnType.GetTypeInfo().IsEnum)
                        {
                            value = Enum.ToObject(propertyInfo.PropertyType, value);
                        }
                        
                            //cast dates from string
                        else if (propertyInfo.PropertyType == typeof (DateTime))
                        {
                            value = value == null ? DateTime.MinValue : DateTime.Parse(value.ToString());
                        }

                         //cast timespan from ticks (int64)
                        else if (propertyInfo.PropertyType == typeof(TimeSpan))
                        {
                            value = value == null ? TimeSpan.MinValue : TimeSpan.FromTicks((Int64)value);
                        }

                        propertyInfo.SetValue(item, value);
                    }
                    items.Add(item);
                }
            }

            return items;
        }

        public Task<List<T>> SelectAllAsync<T>() where T : new()
        {
            return Task.FromResult(SelectAll<T>());
        }

        public Task DeleteTableAsync<T>()
        {
            return Task.Run(() =>
            {
                using (
                    var projstmt =
                        DbConnection.Prepare("DELETE FROM " + typeof (T).Name))
                {
                    projstmt.Step();
                }

                using ( //reset id seed
                    var projstmt =
                        DbConnection.Prepare("DELETE FROM sqlite_sequence  WHERE name = '" + typeof (T).Name + "'"))
                {
                    projstmt.Step();
                }
            });
        }

        public Task DeleteWhereAsync<T>(string property, string value)
        {
            return Task.Run(() =>
            {
                using (
                    var projstmt =
                        DbConnection.Prepare(string.Format("DELETE FROM {0} WHERE {1} = ?", typeof (T).Name, property)))
                {
                    // Reset the prepared statement so we can reuse it.
                    projstmt.ClearBindings();
                    projstmt.Reset();

                    projstmt.Bind(1, value);

                    projstmt.Step();
                }
            });
        }

        private void CreateTablesIfNotExists()
        {
            var sql = EasySql.CreateTable(typeof (Artist));
            using (var statement = DbConnection.Prepare(sql))
            {
                statement.Step();
            }

            sql = EasySql.CreateTable(typeof (Album));
            using (var statement = DbConnection.Prepare(sql))
            {
                statement.Step();
            }

            sql = EasySql.CreateTable(typeof (Song));
            using (var statement = DbConnection.Prepare(sql))
            {
                statement.Step();
            }

            sql = EasySql.CreateTable(typeof (QueueSong));
            using (var statement = DbConnection.Prepare(sql))
            {
                statement.Step();
            }

            sql = EasySql.CreateTable(typeof (Playlist));
            using (var statement = DbConnection.Prepare(sql))
            {
                statement.Step();
            }

            sql = EasySql.CreateTable(typeof (PlaylistSong));
            using (var statement = DbConnection.Prepare(sql))
            {
                statement.Step();
            }

            // Turn on Foreign Key constraints
            sql = @"PRAGMA foreign_keys = ON";
            using (var statement = DbConnection.Prepare(sql))
            {
                statement.Step();
            }

            UpdateDbVersion();
        }

        private void UpdateDbVersion()
        {
            //Set version to one
            var sql = @"PRAGMA user_version = " + CurrentDbVersion;
            using (var statement = DbConnection.Prepare(sql))
            {
                statement.Step();
            }
        }
    }
}