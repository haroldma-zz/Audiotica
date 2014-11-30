using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Audiotica.Data.Model.EchoNest;

namespace Audiotica.Data.Service.Interfaces
{
    public interface IEchoNestService
    {
        Task<EchoArtistUrls> GetArtistUrls(string name);
        Task<EchoBiography> GetArtistBio(string name);
        Task<EchoArtistVideosRoot> GetArtistVideos(string name, int start = 1, int limit = 15);
        Task<EchoArtistImagesRoot> GetArtistImages(string name, int start = 1, int limit = 15);
    }
}
