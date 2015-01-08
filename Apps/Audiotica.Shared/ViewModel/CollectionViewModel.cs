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
using Windows.Storage.Pickers;
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
using Audiotica.Data.Service.Interfaces;
using Audiotica.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using MyToolkit.Utilities;

#endregion

namespace Audiotica.ViewModel
{
    public class CollectionViewModel : ViewModelBase
    {
        private readonly AudioPlayerHelper _audioPlayer;
        private readonly ICollectionService _service;
        private readonly ISongDownloadService _downloadService;
        private ObservableCollection<AlphaKeyGroup<Album>> _sortedAlbums;
        private ObservableCollection<AlphaKeyGroup<Artist>> _sortedArtists;
        private ObservableCollection<AlphaKeyGroup<Song>> _sortedSongs;

        public CollectionViewModel(ICollectionService service, ISongDownloadService downloadService, AudioPlayerHelper audioPlayer)
        {
            _service = service;
            _downloadService = downloadService;
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
            ArtistClickCommand = new RelayCommand<ItemClickEventArgs>(ArtistClickExecute);
            AlbumClickCommand = new RelayCommand<ItemClickEventArgs>(AlbumClickExecute);
            PlaylistClickCommand = new RelayCommand<ItemClickEventArgs>(PlaylistClickExecute);

            AddToClickCommand = new RelayCommand<Song>(AddToClickExecute);
            DeleteClickCommand = new RelayCommand<BaseEntry>(DeleteClickExecute);
            DownloadClickCommand = new RelayCommand<Song>(DownloadClickExecute);
            CancelClickCommand = new RelayCommand<Song>(CancelClickExecute);

            EntryPlayClickCommand = new RelayCommand<BaseEntry>(EntryPlayClickExecute);

            ItemPickedCommand = new RelayCommand<AddableCollectionItem>(ItemPickedExecute);

            CreateBackupCommand = new RelayCommand(CreateBackupExecute);
            RestoreCommand = new RelayCommand(RestoreExecute);
            ImportCommand = new RelayCommand(ImportExecute);
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

        #region Collection item clicked

        private async void SongClickExecute(ItemClickEventArgs e)
        {
            var song = e.ClickedItem as Song;
            var queueSong = _service.Songs.OrderBy(p => p.Name).ToList();
            await CollectionHelper.PlaySongsAsync(song, queueSong);
        }

        private void AlbumClickExecute(ItemClickEventArgs obj)
        {
            var album = obj.ClickedItem as Album;
            App.RootFrame.Navigate(typeof (CollectionAlbumPage), album.Id);
        }

        private void ArtistClickExecute(ItemClickEventArgs obj)
        {
            var artist = obj.ClickedItem as Artist;
            App.RootFrame.Navigate(typeof (CollectionArtistPage), artist.Id);
        }

        private void PlaylistClickExecute(ItemClickEventArgs obj)
        {
            var playlist = obj.ClickedItem as Playlist;
            App.RootFrame.Navigate(typeof (CollectionPlaylistPage), playlist.Id);
        }

        #endregion

        #region Downloading

        private void CancelClickExecute(Song song)
        {
            _downloadService.Cancel(song.Download);
        }

        private void DownloadClickExecute(Song song)
        {
            _downloadService.StartDownloadAsync(song);
        }

        #endregion

        private async void EntryPlayClickExecute(BaseEntry item)
        {
            List<Song> queueSongs = null;

            if (item is Artist)
            {
                var artist = item as Artist;
                queueSongs = artist.Songs.ToList();
            }

            else if (item is Album)
            {
                var album = item as Album;
                queueSongs = album.Songs.ToList();
            }

            else if (item is Playlist)
            {
                var playlist = item as Playlist;
                queueSongs = playlist.Songs.Select(p => p.Song).ToList();
            }

            if (queueSongs != null)
                await CollectionHelper.PlaySongsAsync(queueSongs);
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
        private double _artworkSize = 96;

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

        private async void DeleteClickExecute(BaseEntry item)
        {
            var name = "unknown";

            try
            {
                if (item is Song)
                {
                    var song = item as Song;
                    name = song.Name;

                    await Service.DeleteSongAsync(song);
                }

                else if (item is Playlist)
                {
                    var playlist = item as Playlist;
                    name = playlist.Name;

                    await Service.DeletePlaylistAsync(playlist);
                }

                else if (item is Artist)
                {
                    var artist = item as Artist;
                    name = artist.Name;

                    Service.Artists.Remove(artist);

                    await Task.WhenAll(artist.Songs.ToList().Select(song => Task.WhenAll(new List<Task>
                    {
                        Service.DeleteSongAsync(song)
                    })));
                }

                else if (item is Album)
                {
                    var album = item as Album;
                    name = album.Name;

                    Service.Albums.Remove(album);

                    await Task.WhenAll(album.Songs.ToList().Select(song => Task.WhenAll(new List<Task>
                    {
                        Service.DeleteFromQueueAsync(song),
                        Service.DeleteSongAsync(song)
                    })));
                }

                CurtainPrompt.Show("EntryDeletingSuccess".FromLanguageResource(), name);
            }
            catch
            {
                CurtainPrompt.ShowError("EntryDeletingError".FromLanguageResource(), name);
            }
        }

        private void CreateBackupExecute()
        {
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Audiotica Backup", new List<string>() { ".autcp" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = string.Format("{0}-WP81", (int)DateTime.Now.ToUnixTimeStamp());

            savePicker.PickSaveFileAndContinue();
        }

        private async void RestoreExecute()
        {
            if (await MessageBox.ShowAsync("This will delete all your pre-existing data.", "Continue with Restore?",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            var fileOpenPicker = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
            fileOpenPicker.FileTypeFilter.Add(".autcp");
            fileOpenPicker.PickSingleFileAndContinue();
        }

        private async void ImportExecute()
        {
            UiBlockerUtility.Block("Scanning...");
            var localMusic = await LocalMusicHelper.GetFilesInMusic();

            for (var i = 0; i < localMusic.Count; i++)
            {
                StatusBarHelper.ShowStatus(string.Format("{0} of {1} items added", i + 1, localMusic.Count), (double)i / localMusic.Count);
                await LocalMusicHelper.SaveTrackAsync(localMusic[i]);
            }

            UiBlockerUtility.Unblock();
            await CollectionHelper.DownloadArtistsArtworkAsync();
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
            get { return _artworkSize; }
            set { Set(ref _artworkSize, value); }
        }

        public ICollectionService Service
        {
            get { return _service; }
        }

        public RelayCommand<ItemClickEventArgs> SongClickCommand { get; set; }

        public RelayCommand<ItemClickEventArgs> PlaylistClickCommand { get; set; }

        public RelayCommand<ItemClickEventArgs> AlbumClickCommand { get; set; }

        public RelayCommand<ItemClickEventArgs> ArtistClickCommand { get; set; }


        public RelayCommand<Song> CancelClickCommand { get; set; }

        public RelayCommand<Song> DownloadClickCommand { get; set; }


        public RelayCommand<Song> AddToClickCommand { get; set; }

        public RelayCommand<BaseEntry> DeleteClickCommand { get; set; }

        public RelayCommand<AddableCollectionItem> ItemPickedCommand { get; set; }

        public RelayCommand<BaseEntry> EntryPlayClickCommand { get; set; }

        public RelayCommand CreateBackupCommand { get; set; }
        public RelayCommand RestoreCommand { get; set; }
        public RelayCommand ImportCommand { get; set; }

        #endregion
    }
}