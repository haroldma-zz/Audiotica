using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xbox.Music.Platform.Contract.DataModel;

namespace Audiotica.Data.Service.Interfaces
{
    /// <summary>
    ///     The following interface is use to retieve data from the apis use by the MusicJunkie app.
    /// </summary>
    public interface IXboxMusicService
    {
        Task<XboxPaginatedList<XboxArtist>> GetFeaturedArtist(int count = 10);
        Task<XboxPaginatedList<XboxAlbum>> GetNewAlbums(int count = 10);
        Task<XboxPaginatedList<XboxAlbum>> GetFeaturedAlbums(int count = 10);
    }
}