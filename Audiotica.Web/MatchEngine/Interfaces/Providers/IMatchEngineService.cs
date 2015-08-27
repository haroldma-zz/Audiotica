using System;
using System.Threading.Tasks;

namespace Audiotica.Web.MatchEngine.Interfaces.Providers
{
    /// <summary>
    ///     Provides a wrapper around all available <seealso cref="IMatchProvider" />s.
    /// </summary>
    public interface IMatchEngineService
    {
        Task<Uri> GetLinkAsync(string title, string artist);
    }
}