#region

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SQLite;

#endregion

namespace Audiotica.Data.Collection.DesignTime
{
    public class DesignSqlService : ISqlService
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public SQLiteConnection DbConnection { get; private set; }

        public void Initialize(bool walMode = true, bool readOnlyMode = false)
        {
            throw new NotImplementedException();
        }

        public Task InitializeAsync()
        {
            throw new NotImplementedException();
        }

        public bool Insert(BaseEntry entry)
        {
            throw new NotImplementedException();
        }

        public Task<bool> InsertAsync(BaseEntry entry)
        {
            throw new NotImplementedException();
        }

        public bool DeleteItem(BaseEntry item)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteItemAsync(BaseEntry item)
        {
            throw new NotImplementedException();
        }

        public bool UpdateItem(BaseEntry item)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateItemAsync(BaseEntry item)
        {
            throw new NotImplementedException();
        }

        public T SelectFirst<T>(Func<T, bool> expression) where T : new()
        {
            throw new NotImplementedException();
        }

        public Task<T> SelectFirstAsync<T>(Func<T, bool> expression) where T : new()
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

        public Task DeleteWhereAsync(BaseEntry entry)
        {
            throw new NotImplementedException();
        }

        public void BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }
    }
}