#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Globalization.Collation;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Audiotica.Core;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.SqlHelper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;

#endregion

namespace Audiotica.ViewModel
{
    public class CollectionViewModel : ViewModelBase
    {
        private readonly AudioPlayerHelper _audioPlayer;
        private readonly ICollectionService _service;
        private ObservableCollection<AlphaKeyGroup<Album>> _sortedAlbums;
        private ObservableCollection<AlphaKeyGroup<Artist>> _sortedArtists;
        private ObservableCollection<AlphaKeyGroup<Song>> _sortedSongs;

        public CollectionViewModel(ICollectionService service, AudioPlayerHelper audioPlayer)
        {
            _service = service;
            _audioPlayer = audioPlayer;

            CreateCommand();

            if (IsInDesignModeStatic)
                _service.LoadLibrary();

            SortedSongs = AlphaKeyGroup<Song>.CreateGroups(Service.Songs.ToList(),
                CultureInfo.CurrentCulture, item => item.Name, true);
            SortedArtists = AlphaKeyGroup<Artist>.CreateGroups(Service.Artists.ToList(),
                CultureInfo.CurrentCulture, item => item.Name, true);
            SortedAlbums = AlphaKeyGroup<Album>.CreateGroups(Service.Albums.ToList(),
                CultureInfo.CurrentCulture, item => item.Name, true);

            Service.Songs.CollectionChanged += OnCollectionChanged;
            Service.Albums.CollectionChanged += OnCollectionChanged;
            Service.Artists.CollectionChanged += OnCollectionChanged;

            RandomizeAlbumList = new ObservableCollection<Album>();
        }


        #region Private Helpers 

        private void CreateCommand()
        {
            SongClickCommand = new RelayCommand<ItemClickEventArgs>(SongClickExecute);
            AddToClickCommand = new RelayCommand<Song>(AddToClickExecute);
            DeleteClickCommand = new RelayCommand<Song>(DeleteClickExecute);
            ItemPickedCommand = new RelayCommand<AddableCollectionItem>(ItemPickedExecute);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs arg)
        {
            DispatcherHelper.RunAsync(() =>
            {
                BaseEntry item;
                var removed = false;

                switch (arg.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        item = (BaseEntry) arg.NewItems[0];
                        break;
                    default:
                        item = (BaseEntry) arg.OldItems[0];
                        removed = true;
                        break;
                }

                if (item is Song)
                {
                    var song = item as Song;
                    UpdateSortedCollection(song, removed, song.Name, () => SortedSongs);
                }
                else if (item is Artist)
                {
                    var artist = item as Artist;
                    UpdateSortedCollection(artist, removed, artist.Name, () => SortedArtists);
                }
                else if (item is Album)
                {
                    var album = item as Album;
                    UpdateSortedCollection(album, removed, album.Name, () => SortedAlbums);
                }
            });
        }

        private void UpdateSortedCollection<T>(T item, bool removed, string key,
            Func<ObservableCollection<AlphaKeyGroup<T>>> getSorted)
        {
            if (string.IsNullOrEmpty(key))
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
                        list.Sort(
                            (x, y) => String.Compare(group.OrderKey(x), group.OrderKey(y), StringComparison.Ordinal));
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
            catch { }
        }


        #endregion

        #region Commands (Execute)

        private async void SongClickExecute(ItemClickEventArgs e)
        {
            var song = e.ClickedItem as Song;
            var queueSong = _service.Songs.OrderBy(p => p.Name).ToList();
            await CollectionHelper.PlaySongsAsync(song, queueSong);
        }

        private void AddToClickExecute(Song song)
        {
            song.AddableTo.Clear();
            song.AddableTo.Add(new AddableCollectionItem
            {
                Name = "NowPlayingName".FromLanguageResource()
            });
            song.AddableTo.AddRange(Service
                .Playlists.Where(p => p.Songs.Count(pp => pp.SongId == song.Id) == 0)
                .Select(p => new AddableCollectionItem
                {
                    Playlist = p,
                    Name = p.Name
                }));
            _addableSong = song;
        }

        private Song _addableSong;
        private async void ItemPickedExecute(AddableCollectionItem item)
        {
            if (item.Playlist != null)
            {
                await Service.AddToPlaylistAsync(item.Playlist, _addableSong);
            }
            else
            {
                await CollectionHelper.AddToQueueAsync(_addableSong);
            }
        }

        private async void DeleteClickExecute(Song song)
        {
            try
            {
                //delete from the queue
                await App.Locator.CollectionService.DeleteFromQueueAsync(song);

                await App.Locator.CollectionService.DeleteSongAsync(song);
                CurtainPrompt.Show("EntryDeletingSuccess".FromLanguageResource(), song.Name);
            }
            catch
            {
                CurtainPrompt.ShowError("EntryDeletingError".FromLanguageResource(), song.Name);
            }
        }

        #endregion

        #region Properties

        public ObservableCollection<AlphaKeyGroup<Song>> SortedSongs
        {
            get { return _sortedSongs; }
            set { Set(ref _sortedSongs, value); }
        }

        public ObservableCollection<AlphaKeyGroup<Album>> SortedAlbums
        {
            get { return _sortedAlbums; }
            set { Set(ref _sortedAlbums, value); }
        }

        public ObservableCollection<AlphaKeyGroup<Artist>> SortedArtists
        {
            get { return _sortedArtists; }
            set { Set(ref _sortedArtists, value); }
        }

        public ObservableCollection<Album> RandomizeAlbumList { get; set; }

        public double ArtworkSize
        {
            get
            {
                var w = IsInDesignMode ? 480 : Window.Current.Bounds.Width;
                return w/RowCount;
            }
        }

        public int RowCount
        {
            get
            {
                //extra column if running on hd device (720 and 1080)
                var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
                var actualWidth = Window.Current.Bounds.Width * scaleFactor;
                return actualWidth == 720 || actualWidth == 1080 ? 5 : 4;
            }
        }

        public ICollectionService Service
        {
            get { return _service; }
        }

        public RelayCommand<ItemClickEventArgs> SongClickCommand { get; set; }

        public RelayCommand<Song> AddToClickCommand { get; set; }

        public RelayCommand<Song> DeleteClickCommand { get; set; }

        public RelayCommand<AddableCollectionItem> ItemPickedCommand { get; set; }

        #endregion
    }
}