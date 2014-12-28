#region

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Audiotica.Core.Utilities;
using Audiotica.Data.Model.SoundCloud;
using Audiotica.Data.Service.RunTime;

#endregion

namespace Audiotica.Data.Mp3Providers
{
    public class SoundCloudProvider : IMp3Provider
    {
        public async Task<string> GetMatch(string title, string artist, string album = null)
        {
            var results = await new Mp3SearchService().SearchSoundCloud(title, artist, album).ConfigureAwait(false);
            return results != null && results.Any() ? results.FirstOrDefault().AudioUrl : null;
        }
    }
}