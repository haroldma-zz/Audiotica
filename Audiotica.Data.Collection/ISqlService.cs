#region

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.SqlHelper;
using SQLitePCL;

#endregion

namespace Audiotica.Data.Collection
{
    public interface ISqlService : IDisposable
    {
        void Initialize();
        Task InitializeAsync();
        void ResetData();

        SQLiteResult Insert(BaseEntry entry);
        Task<SQLiteResult> InsertAsync(BaseEntry entry);

        SQLiteResult DeleteItem(BaseEntry item);
        Task<SQLiteResult> DeleteItemAsync(BaseEntry item);
        SQLiteResult UpdateItem(BaseEntry item);
        Task<SQLiteResult> UpdateItemAsync(BaseEntry item);


        List<T> SelectAll<T>() where T : new();
        Task<List<T>> SelectAllAsync<T>() where T : new();

        Task DeleteTableAsync<T>();
        Task DeleteWhereAsync<T>(string property, string value);
    }
}