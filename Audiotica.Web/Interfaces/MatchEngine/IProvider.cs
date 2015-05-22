using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Audiotica.Web.Models;

namespace Audiotica.Web.Interfaces.MatchEngine
{
    public enum ProviderSpeed
    {
        SuperSlow = -2,
        Slow = -1,
        Average,
        Fast
    }

    public enum ProviderResultsQuality
    {
        BetterThanNothing = 1,
        NotSoGreat,
        SomewhatGreat,
        Great,
        Excellent
    }

    public interface IProvider
    {
        bool IsEnabled { get; set; }
        int Priority { get; }
        string DisplayName { get; }
        ProviderSpeed Speed { get; }
        ProviderResultsQuality ResultsQuality { get; }
        Task<Uri> GetLinkAsync(string title, string artist);
        Task<List<WebSong>> GetSongsAsync(string title, string artist, int limit = 10);
        void Enable();
        void Disable();
    }
}