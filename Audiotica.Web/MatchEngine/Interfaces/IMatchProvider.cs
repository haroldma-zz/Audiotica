using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;

namespace Audiotica.Web.MatchEngine.Interfaces
{
    public interface IMatchProvider : IConfigurableProvider
    {
        Task<Uri> GetLinkAsync(string title, string artist);
        Task<List<MatchSong>> GetSongsAsync(string title, string artist, int limit = 10);
    }
}