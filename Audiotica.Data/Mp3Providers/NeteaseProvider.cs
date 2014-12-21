#region

using System.Linq;
using System.Threading.Tasks;
using Audiotica.Data.Service.RunTime;

#endregion

namespace Audiotica.Data.Mp3Providers
{
    public class NeteaseProvider : IMp3Provider
    {
        public async Task<string> GetMatch(string title, string artist, string album = null)
        {
            var results = await new Mp3SearchService().SearchNetease(title, artist, album);
            return results != null && results.Any() ? results.FirstOrDefault().AudioUrl : null;
        }
    }
}