#region

using System.Collections.Generic;
using System.Threading.Tasks;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.SqlHelper;

#endregion

namespace Audiotica.Data.Collection
{
    public interface ISqlService
    {
        void Initialize();
        Task InitializeAsync();
        void ResetData();

        Task InsertAsync(BaseEntry entry);

        Task DeleteItemAsync(BaseEntry item);
        Task UpdateItemAsync(BaseEntry item);

        List<T> SelectAll<T>() where T : new();
        Task<List<T>> SelectAllAsync<T>() where T : new();

        Task DeleteTableAsync<T>();
        Task DeleteWhereAsync<T>(string property, string value);
    }
}