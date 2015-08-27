using Audiotica.Core.Common;
using Audiotica.Database.Models;
using Audiotica.Windows.Tools;

namespace Audiotica.Windows.Services.Interfaces
{
    public interface ILibraryCollectionService
    {
        OptimizedObservableCollection<AlphaKeyGroup> ArtistsByName { get; }
        OptimizedObservableCollection<Album> AlbumsByDateAdded { get; }
        OptimizedObservableCollection<AlphaKeyGroup> AlbumsByTitle { get; }
        OptimizedObservableCollection<AlphaKeyGroup> AlbumsByArtist { get; }
        OptimizedObservableCollection<Track> TracksByDateAdded { get; }
        OptimizedObservableCollection<AlphaKeyGroup> TracksByTitle { get; }
        OptimizedObservableCollection<AlphaKeyGroup> TracksByArtist { get; }
        OptimizedObservableCollection<AlphaKeyGroup> TracksByAlbum { get; }
    }
}