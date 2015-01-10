using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.SqlHelper;
using SQLitePCL;

namespace Audiotica.Data.Collection.DesignTime
{
    public class DesignSqlService : ISqlService
    {
        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public Task InitializeAsync()
        {
            throw new NotImplementedException();
        }

        public void ResetData()
        {
            throw new NotImplementedException();
        }

        public SQLiteResult Insert(BaseEntry entry)
        {
            throw new NotImplementedException();
        }

        public Task<SQLiteResult> InsertAsync(BaseEntry entry)
        {
            throw new NotImplementedException();
        }

        public SQLiteResult DeleteItem(BaseEntry item)
        {
            throw new NotImplementedException();
        }

        public Task InsertPlaylistSongAsync(PlaylistSong song)
        {
            throw new NotImplementedException();
        }

        public Task<SQLiteResult> DeleteItemAsync(BaseEntry item)
        {
            throw new NotImplementedException();
        }

        public SQLiteResult UpdateItem(BaseEntry item)
        {
            throw new NotImplementedException();
        }

        public Task<SQLiteResult> UpdateItemAsync(BaseEntry queue)
        {
            throw new NotImplementedException();
        }

        public T SelectWhere<T>(string property, string value) where T : new()
        {
            throw new NotImplementedException();
        }


        public List<T> SelectAll<T>() where T : new()
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> SelectAllAsync<T>() where T : new()
        {
            throw new NotImplementedException();
        }

        public Task DeleteTableAsync<T>()
        {
            throw new NotImplementedException();
        }

        public Task DeleteWhereAsync<T>(string property, string value)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
