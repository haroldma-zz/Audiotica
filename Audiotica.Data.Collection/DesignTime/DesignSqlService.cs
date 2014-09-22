using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.SqlHelper;

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

        public Task InsertAsync(BaseEntry entry)
        {
            throw new NotImplementedException();
        }

        public Task InsertPlaylistSongAsync(PlaylistSong song)
        {
            throw new NotImplementedException();
        }

        public Task DeleteItemAsync(BaseEntry item)
        {
            throw new NotImplementedException();
        }

        public Task UpdateItemAsync(BaseEntry queue)
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
    }
}
