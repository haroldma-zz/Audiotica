#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Audiotica.Data.Collection.SqlHelper;
using SQLitePCL;

#endregion

namespace Audiotica.Data.Collection.RunTime
{
    public class SqlService : ISqlService
    {
        private readonly SqlServiceConfig _config;

        public SqlService(SqlServiceConfig config)
        {
            _config = config;
            DbConnection = new SQLiteConnection(config.Path);
            Initialize();
        }

        public SQLiteConnection DbConnection { get; set; }

        public void Dispose()
        {
            DbConnection.Dispose();
        }

        public void Initialize()
        {
            long sqlVersion;

            using (var statement = DbConnection.Prepare("PRAGMA user_version"))
            {
                statement.Step();
                sqlVersion = (long) statement[0];
            }

            if (sqlVersion == _config.CurrentVersion) return;

            CreateTablesIfNotExists();
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

        public SQLiteResult Insert(BaseEntry entry)
        {
            SQLiteResult res;
            using (var custstmt = DbConnection.Prepare(EasySql.CreateInsert(entry.GetType())))
            {
                EasySql.FillInsert(custstmt, entry);
                bool retry;
                do
                {
                    res = custstmt.Step();
                    retry = res == SQLiteResult.BUSY;
                } while (retry);
            }

            if (res != SQLiteResult.DONE) return res;

            using (var idstmt = DbConnection.Prepare("SELECT last_insert_rowid()"))
            {
                idstmt.Step();
                {
                    entry.Id = (long) idstmt[0];
                }
            }

            return res;
        }

        public Task<SQLiteResult> InsertAsync(BaseEntry entry)
        {
            return Task.FromResult(Insert(entry));
        }

        public SQLiteResult DeleteItem(BaseEntry item)
        {
            using (
                var projstmt =
                    DbConnection.Prepare(string.Format("DELETE FROM {0} WHERE Id = ?", item.GetType().Name)))
            {
                // Reset the prepared statement so we can reuse it.
                projstmt.ClearBindings();
                projstmt.Reset();

                projstmt.Bind(1, item.Id);

                SQLiteResult result;
                bool retry;
                do
                {
                    result = projstmt.Step();
                    retry = result == SQLiteResult.BUSY;
                } while (retry);

                return result;
            }
        }

        public Task<SQLiteResult> DeleteItemAsync(BaseEntry item)
        {
            return Task.FromResult(DeleteItem(item));
        }

        public SQLiteResult UpdateItem(BaseEntry item)
        {
            using (
                var projstmt =
                    DbConnection.Prepare(EasySql.CreateUpdate(item.GetType())))
            {
                // Reset the prepared statement so we can reuse it.
                projstmt.ClearBindings();
                projstmt.Reset();

                EasySql.FillUpdate(projstmt, item);

                SQLiteResult res;
                bool retry;
                do
                {
                    res = projstmt.Step();
                    retry = res == SQLiteResult.BUSY;
                } while (retry);

                return res;
            }
        }

        public Task<SQLiteResult> UpdateItemAsync(BaseEntry item)
        {
            return Task.FromResult(UpdateItem(item));
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
                        else if (propertyInfo.PropertyType == typeof (TimeSpan))
                        {
                            value = value == null ? TimeSpan.MinValue : TimeSpan.FromTicks((Int64) value);
                        }

                        else if (propertyInfo.PropertyType == typeof (bool))
                        {
                            value = (long) value == 1;
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
            foreach (var sql in _config.Tables.Select(EasySql.CreateTable))
            {
                using (var statement = DbConnection.Prepare(sql))
                {
                    statement.Step();
                }
            }

            // Turn on Foreign Key constraints
            using (var statement = DbConnection.Prepare("PRAGMA foreign_keys = ON"))
            {
                statement.Step();
            }

            UpdateDbVersion(_config.CurrentVersion);
        }

        private void UpdateDbVersion(double version)
        {
            //Set version to one
            var sql = @"PRAGMA user_version = " + version;
            using (var statement = DbConnection.Prepare(sql))
            {
                statement.Step();
            }
        }
    }

    public class SqlServiceConfig
    {
        public double CurrentVersion { get; set; }
        public string Path { get; set; }
        public List<Type> Tables { get; set; }
    }
}