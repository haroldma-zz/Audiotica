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
    public class SqlService : ISqlService
    {
        public SQLiteConnection CreateConnection()
        {
            return new SQLiteConnection("collection.sqldb");
        }

        public void Initialize()
        {
            using (var db = CreateConnection())
            {
                var sql = EasySql.CreateTable(typeof (Artist));
                using (var statement = db.Prepare(sql))
                {
                    statement.Step();
                }

                sql = EasySql.CreateTable(typeof (Album));
                using (var statement = db.Prepare(sql))
                {
                    statement.Step();
                }

                sql = EasySql.CreateTable(typeof (Song));
                using (var statement = db.Prepare(sql))
                {
                    statement.Step();
                }

                sql = EasySql.CreateTable(typeof (QueueSong));
                using (var statement = db.Prepare(sql))
                {
                    statement.Step();
                }

                sql = EasySql.CreateTable(typeof (Playlist));
                using (var statement = db.Prepare(sql))
                {
                    statement.Step();
                }

                sql = EasySql.CreateTable(typeof (PlaylistSong));
                using (var statement = db.Prepare(sql))
                {
                    statement.Step();
                }

                // Turn on Foreign Key constraints
                sql = @"PRAGMA foreign_keys = ON";
                using (var statement = db.Prepare(sql))
                {
                    statement.Step();
                }
            }
        }

        public Task InitializeAsync()
        {
            return Task.Factory.StartNew(Initialize);
        }

        public void ResetData()
        {
            using (var db = CreateConnection())
            {
                var sql = @"DELETE FROM Song";
                using (var statement = db.Prepare(sql))
                {
                    statement.Step();
                }

                sql = @"DELETE FROM Album";
                using (var statement = db.Prepare(sql))
                {
                    statement.Step();
                }

                sql = @"DELETE FROM Artist";
                using (var statement = db.Prepare(sql))
                {
                    statement.Step();
                }

                sql = @"DELETE FROM QueueSong";
                using (var statement = db.Prepare(sql))
                {
                    statement.Step();
                }

                sql = @"DELETE FROM PlaylistSong";
                using (var statement = db.Prepare(sql))
                {
                    statement.Step();
                }
            }
        }


        public async Task InsertAsync(BaseEntry entry)
        {
            using (var db = CreateConnection())
            {
                try
                {
                    await Task.Run(() =>
                    {
                        using (var custstmt = db.Prepare(EasySql.CreateInsert(entry.GetType())))
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

                using (var idstmt = db.Prepare("SELECT last_insert_rowid()"))
                {
                    idstmt.Step();
                    {
                        entry.Id = (long) idstmt[0];
                    }
                }
            }
        }

        public Task DeleteItemAsync(BaseEntry item)
        {
            return Task.Run(() =>
            {
                using (var db = CreateConnection())
                {
                    using (
                        var projstmt =
                            db.Prepare(string.Format("DELETE FROM {0} WHERE Id = ?", item.GetType().Name)))
                    {
                        // Reset the prepared statement so we can reuse it.
                        projstmt.ClearBindings();
                        projstmt.Reset();

                        projstmt.Bind(1, item.Id);

                        projstmt.Step();
                    }
                }
            });
        }

        public Task UpdateItemAsync(BaseEntry item)
        {
            return Task.Run(() =>
            {
                using (var db = CreateConnection())
                {
                    using (
                        var projstmt =
                            db.Prepare(EasySql.CreateUpdate(item.GetType())))
                    {
                        // Reset the prepared statement so we can reuse it.
                        projstmt.ClearBindings();
                        projstmt.Reset();

                        EasySql.FillUpdate(projstmt, item);

                        projstmt.Step();
                    }
                }
            });
        }

        public List<T> SelectAll<T>() where T : new()
        {
            using (var db = CreateConnection())
            {
                var type = typeof (T);
                var items = new List<T>();

                using (var statement = db.Prepare("SELECT * FROM " + type.Name))
                {
                    while (statement.Step() == SQLiteResult.ROW)
                    {
                        var item = new T();
                        var props = type.GetRuntimeProperties().Where(p => p.GetCustomAttribute<SqlIgnore>() == null);

                        foreach (var propertyInfo in props)
                        {
                            object value = statement[propertyInfo.Name];

                            //cast enums from long
                            if (propertyInfo.GetMethod.ReturnType.GetTypeInfo().IsEnum)
                                value = Enum.ToObject(propertyInfo.PropertyType, value);
                        
                                //cast dates from string
                            else if (propertyInfo.PropertyType == typeof (DateTime))
                                value = DateTime.Parse(value.ToString());

                            propertyInfo.SetValue(item, value);
                        }
                        items.Add(item);
                    }
                }

                return items;
            }
        }

        public Task<List<T>> SelectAllAsync<T>() where T : new()
        {
            return Task.FromResult(SelectAll<T>());
        }

        public Task DeleteTableAsync<T>()
        {
            return Task.Run(() =>
            {
                using (var db = CreateConnection())
                {
                    using (
                        var projstmt =
                            db.Prepare("DELETE FROM " + typeof (T).Name))
                    {
                        projstmt.Step();
                    }

                    using ( //reset id seed
                        var projstmt =
                            db.Prepare("DELETE FROM sqlite_sequence  WHERE name = '" + typeof (T).Name + "'"))
                    {
                        projstmt.Step();
                    }
                }
            });
        }
    }
}