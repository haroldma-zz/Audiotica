using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using Windows.Globalization.Collation;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Tools;
using static System.String;

namespace Audiotica.Windows.Services.RunTime
{
    public class LibraryCollectionService
    {
        private readonly IDispatcherUtility _dispatcherUtility;
        private readonly ILibraryService _libraryService;

        public LibraryCollectionService(ILibraryService libraryService, IDispatcherUtility dispatcherUtility)
        {
            _libraryService = libraryService;
            _dispatcherUtility = dispatcherUtility;

            Configure();
        }

        private void Configure()
        {
            TracksByDateAdded = new OptimizedObservableCollection<Track>(
                _libraryService.Tracks.OrderByDescending(p => p.CreatedAt));
            TracksByTitle = AlphaKeyGroup<Track>.CreateGroups(_libraryService.Tracks, CultureInfo.CurrentCulture,
                item => item.Title);
            TracksByArtist = AlphaKeyGroup<Track>.CreateGroups(_libraryService.Tracks, CultureInfo.CurrentCulture,
                item => item.DisplayArtist);
            TracksByAlbum = AlphaKeyGroup<Track>.CreateGroups(_libraryService.Tracks, CultureInfo.CurrentCulture,
                item => item.AlbumTitle);
            
            ArtistsByName = AlphaKeyGroup<Artist>.CreateGroups(_libraryService.Artists, CultureInfo.CurrentCulture,
                item => item.Name);

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
                    /*if (artist.Tracks.Count > 0 || removed)
                        UpdateSortedCollection(artist, removed, artist.Name, () => SortedArtists);*/
                }
                else if (item is Album)
                {
                    var album = item as Album;
                    /* if (album.Tracks.Count > 0 || removed)
                        UpdateSortedCollection(album, removed, album.Name, () => SortedAlbums);*/
                }
            });
        }

        private void ResetSortedCollections()
        {
            var dateAdded = new OptimizedObservableCollection<Track>(
                _libraryService.Tracks.OrderByDescending(p => p.CreatedAt));
            TracksByDateAdded.SwitchTo(dateAdded);

            var tracksByTitle = AlphaKeyGroup<Track>.CreateGroups(_libraryService.Tracks, CultureInfo.CurrentCulture,
                item => item.Title);
            TracksByTitle.SwitchTo(tracksByTitle);

            var tracksByArtist = AlphaKeyGroup<Track>.CreateGroups(_libraryService.Tracks, CultureInfo.CurrentCulture,
                item => item.DisplayArtist);
            TracksByArtist.SwitchTo(tracksByArtist);

            var tracksByAlbum = AlphaKeyGroup<Track>.CreateGroups(_libraryService.Tracks, CultureInfo.CurrentCulture,
                item => item.AlbumTitle);
            TracksByAlbum.SwitchTo(tracksByAlbum);
        }

        private void UpdateSortedCollection<T>(T item, bool removed, string key,
            Func<OptimizedObservableCollection<AlphaKeyGroup<T>>> getSorted)
        {
            if (IsNullOrEmpty(key))
                return;

            var sortedGroups = getSorted();
            try
            {
                var charKey = new CharacterGroupings().Lookup(key);
                var group = sortedGroups.First(a => a.Key == charKey);

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
        public OptimizedObservableCollection<AlphaKeyGroup<Track>> TracksByTitle { get; private set; }
        public OptimizedObservableCollection<AlphaKeyGroup<Track>> TracksByArtist { get; private set; }
        public OptimizedObservableCollection<AlphaKeyGroup<Track>> TracksByAlbum { get; private set; }

        #endregion

        #region Artists
        
        public OptimizedObservableCollection<AlphaKeyGroup<Artist>> ArtistsByName { get; private set; }

        #endregion
    }
}