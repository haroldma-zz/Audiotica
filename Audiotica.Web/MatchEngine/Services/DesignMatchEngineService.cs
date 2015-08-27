using System;
using System.Threading.Tasks;
using Audiotica.Web.MatchEngine.Interfaces.Providers;

namespace Audiotica.Web.MatchEngine.Services
{
    public class DesignMatchEngineService : IMatchEngineService
    {
        public Task<Uri> GetLinkAsync(string title, string artist)
        {
            return null;
        }
    }
}