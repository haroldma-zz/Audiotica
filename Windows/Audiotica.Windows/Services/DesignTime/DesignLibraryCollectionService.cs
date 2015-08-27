using System.Globalization;
using System.Linq;
using Audiotica.Core.Common;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.Tools;

namespace Audiotica.Windows.Services.DesignTime
{
    public class DesignLibraryCollectionService : ILibraryCollectionService
    {
        public DesignLibraryCollectionService(ILibraryService libraryService)
        {
            TracksByDateAdded = new OptimizedObservableCollection<Track>(
                libraryService.Tracks.OrderByDescending(p => p.CreatedAt));
            TracksByTitle = AlphaKeyGroup.CreateGroups(libraryService.Tracks, CultureInfo.CurrentCulture,
                item => ((Track) item).Title);
            TracksByArtist = AlphaKeyGroup.CreateGroups(libraryService.Tracks, CultureInfo.CurrentCulture,
                item => ((Track) item).DisplayArtist);
            TracksByAlbum = AlphaKeyGroup.CreateGroups(libraryService.Tracks, CultureInfo.CurrentCulture,
                item => ((Track) item).AlbumTitle);

            ArtistsByName = AlphaKeyGroup.CreateGroups(
                libraryService.Artists.Where(p => !p.IsSecondaryArtist), CultureInfo.CurrentCulture,
                item => ((Artist) item).Name);

            AlbumsByTitle = AlphaKeyGroup.CreateGroups(libraryService.Albums, CultureInfo.CurrentCulture,
                item => ((Album) item).Title);
        }

        public OptimizedObservableCollection<AlphaKeyGroup> ArtistsByName { get; }
        public OptimizedObservableCollection<Album> AlbumsByDateAdded { get; }
        public OptimizedObservableCollection<AlphaKeyGroup> AlbumsByTitle { get; }
        public OptimizedObservableCollection<AlphaKeyGroup> AlbumsByArtist { get; }
        public OptimizedObservableCollection<Track> TracksByDateAdded { get; }
        public OptimizedObservableCollection<AlphaKeyGroup> TracksByTitle { get; }
        public OptimizedObservableCollection<AlphaKeyGroup> TracksByArtist { get; }
        public OptimizedObservableCollection<AlphaKeyGroup> TracksByAlbum { get; }
    }
}