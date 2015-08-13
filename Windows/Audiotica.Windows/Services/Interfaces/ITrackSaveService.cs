using System;
using System.Threading.Tasks;
using Audiotica.Database.Models;
using Audiotica.Web.Models;

namespace Audiotica.Windows.Services.Interfaces
{
    public interface ITrackSaveService
    {
        Task<Track> SaveAsync(WebSong song);
        Task SaveAsync(Track track);
    }
}