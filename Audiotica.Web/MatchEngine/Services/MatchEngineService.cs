using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Web.MatchEngine.Interfaces;
using Audiotica.Web.MatchEngine.Interfaces.Providers;

namespace Audiotica.Web.MatchEngine.Services
{
    public class MatchEngineService : IMatchEngineService
    {
        private readonly List<IMatchProvider> _providers;

        public MatchEngineService(IEnumerable<IMatchProvider> providers)
        {
            _providers = providers.Where(p => p.IsEnabled).OrderByDescending(p => p.Priority).ToList();
        }

        public async Task<Uri> GetLinkAsync(string title, string artist)
        {
            foreach (var provider in _providers)
            {
                try
                {
                    var uri = await provider.GetLinkAsync(title, artist);
                    if (uri != null) return uri;
                }
                catch
                {
                    // ignored
                }
            }
            return null;
        }
    }
}