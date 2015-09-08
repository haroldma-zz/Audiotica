using Audiotica.Database.Models;

namespace Audiotica.Windows.Services.Interfaces
{
    internal interface ILibraryMatchingService
    {
        void OnStartup();
        void Queue(Track track);
    }
}