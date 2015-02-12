#region

using System.Collections.Generic;
using System.Threading.Tasks;
using Audiotica.Data.Model;

#endregion

namespace Audiotica.Data.Service.Interfaces
{
    public interface IMp3SearchService
    {
        Task<List<WebSong>> SearchSoundCloud(string title, string artist, string album = null, int limit = 5, bool checkAllLinks = false);
        Task<List<WebSong>> SearchMp3Clan(string title, string artist, string album = null, int limit = 5, bool checkAllLinks = false);
        Task<List<WebSong>> SearchMp3Truck(string title, string artist, string album = null, bool checkAllLinks = false);
        Task<List<WebSong>> SearchMeile(string title, string artist, string album = null, int limit = 5, bool checkAllLinks = false);
        Task<List<WebSong>> SearchNetease(string title, string artist, string album = null, int limit = 5, bool checkAllLinks = false);

        Task<int> GetBitrateFromCc(string ccId);
    }
}