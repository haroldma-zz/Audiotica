using System.Threading.Tasks;
using Audiotica.Database.Models;
using Audiotica.Web.Models;
using TagLib;

namespace Audiotica.Windows.Services.Interfaces
{
    public interface ITrackSaveService
    {
        Task<Track> SaveAsync(WebSong song);

        Task SaveAsync(Track track);

        Task SaveAsync(Track track, Tag tag);
    }
}