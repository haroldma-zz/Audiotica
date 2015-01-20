#region

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Audiotica.Data.Collection.SqlHelper;
using SQLite;

#endregion

namespace Audiotica.Data.Collection
{
    public interface ISqlService : IDisposable
    {
        SQLiteConnection DbConnection { get; }

        void Initialize(bool walMode = true);
        Task InitializeAsync();

        bool Insert(BaseEntry entry);
        Task<bool> InsertAsync(BaseEntry entry);

        bool DeleteItem(BaseEntry item);
        Task<bool> DeleteItemAsync(BaseEntry item);
        bool UpdateItem(BaseEntry item);
        Task<bool> UpdateItemAsync(BaseEntry item);


        T SelectFirst<T>(Func<T, bool> expression) where T : new();
        List<T> SelectAll<T>() where T : new();
        Task<List<T>> SelectAllAsync<T>() where T : new();

        Task DeleteTableAsync<T>();
        Task DeleteWhereAsync(BaseEntry entry);
    }
}