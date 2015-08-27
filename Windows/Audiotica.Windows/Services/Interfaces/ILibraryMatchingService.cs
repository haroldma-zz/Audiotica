using Audiotica.Database.Models;

namespace Audiotica.Windows.Services.Interfaces
{
    internal interface ILibraryMatchingService
    {
        bool IsMatching { get; }
        void OnStartup();
        void Queue(Track track);
    }
}