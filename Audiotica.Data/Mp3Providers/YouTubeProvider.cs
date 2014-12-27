#region

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Audiotica.Core.Utilities;
using Audiotica.Data.Model;
using Audiotica.Data.Service.RunTime;
using HtmlAgilityPack;

#endregion

namespace Audiotica.Data.Mp3Providers
{
    public class YouTubeProvider : IMp3Provider
    {
        public async Task<string> GetMatch(string title, string artist, string album = null)
        {
            var results = await new Mp3SearchService().SearchYoutube(title, artist, album);
            return results != null && results.Any() ? results.FirstOrDefault().AudioUrl : null;
        }
    }
}
