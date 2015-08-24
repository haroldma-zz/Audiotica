using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using Windows.Globalization.Collation;
using Audiotica.Core.Common;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Services.Interfaces;
using Audiotica.Windows.Tools;
using static System.String;

namespace Audiotica.Windows.Services.RunTime
{
    public class LibraryCollectionService : ILibraryCollectionService
    {
        private readonly IDispatcherUtility _dispatcherUtility;
        private readonly ILibraryService _libraryService;

        public LibraryCollectionService(ILibraryService libraryService, IDispatcherUtility dispatcherUtility)
        {
            _libraryService = libraryService;
            _dispatcherUtility = dispatcherUtility;

            Configure();
        }

        #region Artists

        public OptimizedObservableCollection<AlphaKeyGroup> ArtistsByName { get; private set; }

        #endregion

        #region Albums

        public OptimizedObservableCollection<AlphaKeyGroup> AlbumsByTitle { get; private set; }

        #endregion

        private void Configure()
        {
            TracksByDateAdded = new OptimizedObservableCollection<Track>(
                _libraryService.Tracks.OrderByDescending(p => p.CreatedAt));
            TracksByTitle = AlphaKeyGroup.CreateGroups(_libraryService.Tracks, CultureInfo.CurrentCulture,
                item => ((Track) item).Title);
            TracksByArtist = AlphaKeyGroup.CreateGroups(_libraryService.Tracks, CultureInfo.CurrentCulture,
                item => ((Track) item).DisplayArtist);
            TracksByAlbum = AlphaKeyGroup.CreateGroups(_libraryService.Tracks, CultureInfo.CurrentCulture,
                item => ((Track) item).AlbumTitle);

            ArtistsByName = AlphaKeyGroup.CreateGroups(
                _libraryService.Artists.Where(p => !p.IsSecondaryArtist), CultureInfo.CurrentCulture,
                item => ((Artist) item).Name);

            AlbumsByTitle = AlphaKeyGroup.CreateGroups(_libraryService.Albums, CultureInfo.CurrentCulture,
                item => ((Album) item).Title);

            _libraryService.Tracks.CollectionChanged += OnCollectionChanged;
            _libraryService.Artists.CollectionChanged += OnCollectionChanged;
            _libraryService.Albums.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs arg)
        {
            _dispatcherUtility.RunAsync(() =>
            {
                object item;
                var removed = false;

                switch (arg.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                        ResetSortedCollections();
                        return;
                    case NotifyCollectionChangedAction.Add:
                        item = arg.NewItems[0];
                        break;
                    default:
                        item = arg.OldItems[0];
                        removed = true;
                        break;
                }

                if (item is Track)
                {
                    var song = item as Track;

                    if (removed)
                        TracksByDateAdded.Remove(song);
                    else
                        TracksByDateAdded.Insert(0, song);

                    UpdateSortedCollection(song, removed, song.Title, () => TracksByTitle);
                    UpdateSortedCollection(song, removed, song.DisplayArtist, () => TracksByArtist);
                    UpdateSortedCollection(song, removed, song.AlbumTitle, () => TracksByAlbum);
                }
                else if (item is Artist)
                {
                    var artist = item as Artist;

                    if (removed)
                        artist.Tracks.CollectionChanged -= TracksOnCollectionChanged;
                    else
                        artist.Tracks.CollectionChanged += TracksOnCollectionChanged;

                    if (!artist.IsSecondaryArtist || removed)
                        UpdateSortedCollection(artist, removed, artist.Name, () => ArtistsByName);
                }
                else if (item is Album)
                {
                    var album = item as Album;
                    UpdateSortedCollection(album, removed, album.Title, () => AlbumsByTitle);
                }
            });
        }

        /// <summary>
        ///     Secondary artist are not shown
        ///     but we are listening for changes and they are added if they end up getting tracks
        /// </summary>
        private void TracksOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            var tracks = (OptimizedObservableCollection<Track>) sender;
            var remove = tracks.Count == 0;
            var artist = _libraryService.Artists.FirstOrDefault(p => p.Tracks == tracks);

            if (artist == null)
                return;

            UpdateSortedCollection(artist, remove, artist.Name, () => ArtistsByName);
        }

        private void ResetSortedCollections()
        {
            var dateAdded = new OptimizedObservableCollection<Track>(
                _libraryService.Tracks.OrderByDescending(p => p.CreatedAt));
            TracksByDateAdded.SwitchTo(dateAdded);

            var tracksByTitle = AlphaKeyGroup.CreateGroups(_libraryService.Tracks, CultureInfo.CurrentCulture,
                item => ((Track) item).Title);
            TracksByTitle.SwitchTo(tracksByTitle);

            var tracksByArtist = AlphaKeyGroup.CreateGroups(_libraryService.Tracks, CultureInfo.CurrentCulture,
                item => ((Track) item).DisplayArtist);
            TracksByArtist.SwitchTo(tracksByArtist);

            var tracksByAlbum = AlphaKeyGroup.CreateGroups(_libraryService.Tracks, CultureInfo.CurrentCulture,
                item => ((Track) item).AlbumTitle);
            TracksByAlbum.SwitchTo(tracksByAlbum);

            var artistsByName = AlphaKeyGroup.CreateGroups(
                _libraryService.Artists.Where(p => !p.IsSecondaryArtist), CultureInfo.CurrentCulture,
                item => ((Artist) item).Name);
            ArtistsByName = artistsByName;

            var albumsByTitle = AlphaKeyGroup.CreateGroups(_libraryService.Albums, CultureInfo.CurrentCulture,
                item => ((Album) item).Title);
            AlbumsByTitle = albumsByTitle;
        }

        private void UpdateSortedCollection<T>(T item, bool removed, string key,
            Func<OptimizedObservableCollection<AlphaKeyGroup>> getSorted)
        {
            if (IsNullOrEmpty(key))
                return;

            var sortedGroups = getSorted();
            try
            {
                var charKey = new CharacterGroupings().Lookup(key);
                var group = sortedGroups.FirstOrDefault(a => a.Key.EqualsIgnoreCase(charKey));

                if (!removed && group.Contains(item))
                    return;
                if (removed && !group.Contains(item))
                    return;

                bool zero;
                if (removed)
                {
                    group.Remove(item);
                    zero = group.Count == 0;
                }

                else
                {
                    zero = group.Count == 0;
                    var index = 0;

                    //if the group is not empty, then insert acording to sort
                    if (!zero)
                    {
                        var list = group.ToList();
                        list.Add(item);
                        list.Sort((x, y) =>
                            Compare(group.OrderKey(x), group.OrderKey(y), StringComparison.Ordinal));
                        index = list.IndexOf(item);
                    }
                    group.Insert(index, item);
                }

                if (!zero) return;

                //removing and readding to update the groups collection in the listview
                var groupIndex = sortedGroups.IndexOf(group);
                sortedGroups.Remove(group);
                sortedGroups.Insert(groupIndex, group);
            }
            catch
            {
                // ignored
            }
        }

        #region Tracks

        public OptimizedObservableCollection<Track> TracksByDateAdded { get; private set; }
        public OptimizedObservableCollection<AlphaKeyGroup> TracksByTitle { get; private set; }
        public OptimizedObservableCollection<AlphaKeyGroup> TracksByArtist { get; private set; }
        public OptimizedObservableCollection<AlphaKeyGroup> TracksByAlbum { get; private set; }

        #endregion
    }
}