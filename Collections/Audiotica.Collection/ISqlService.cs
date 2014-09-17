using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Audiotica.Collection
{
    public interface ISqlService
    {
        SQLiteConnection Connection { get; }

        void Initialize();

        Task InitializeAsync();

        Task<List<T>> GetAllAsync<T>() where T : new();

        Task<List<T>> GetWhereAsync<T>(Func<T, bool> predicate) where T : new();

        Task InsertAsync<T>(T obj);

        Task UpdateAsync<T>(T obj);

        Task DeleteAsync<T>(T obj);

        Task DeleteAllAsync<T>();

        Task<T> GetFirstAsync<T>(Func<T, bool> predicate) where T : new();
    }
}
