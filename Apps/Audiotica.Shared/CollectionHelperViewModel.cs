using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.SqlHelper;
using Audiotica.Data.Service.Interfaces;
using Audiotica.View;
using GalaSoft.MvvmLight.Command;

namespace Audiotica
{
    public class CollectionCommandHelper
    {
        private readonly ICollectionService _service;
        private readonly ISqlService _sqlService;
        private readonly ISongDownloadService _downloadService;

        public CollectionCommandHelper(ICollectionService service, ISqlService sqlService, ISongDownloadService downloadService)
        {
            _service = service;
            _sqlService = sqlService;
            _downloadService = downloadService;

            CreateCommand();
        }

        private void CreateCommand()
        {
            ArtistClickCommand = new RelayCommand<ItemClickEventArgs>(ArtistClickExecute);
            AlbumClickCommand = new RelayCommand<ItemClickEventArgs>(AlbumClickExecute);
            PlaylistClickCommand = new RelayCommand<ItemClickEventArgs>(PlaylistClickExecute);

            AddToClickCommand = new RelayCommand<BaseEntry>(AddToClickExecute);

            DeleteClickCommand = new RelayCommand<BaseEntry>(DeleteClickExecute);
            DownloadClickCommand = new RelayCommand<Song>(DownloadClickExecute);
            CancelClickCommand = new RelayCommand<Song>(CancelClickExecute);

            EntryPlayClickCommand = new RelayCommand<BaseEntry>(EntryPlayClickExecute);

            ItemPickedCommand = new RelayCommand<AddableCollectionItem>(ItemPickedExecute);
        }

        private void ExitIfArtistEmpty(Artist artist)
        {
            if (App.Navigator.CurrentPage is CollectionArtistPage && artist.Songs.Count == 0)
            {
                App.Navigator.GoBack();
            }
        }

        private void ExitIfAlbumEmpty(Album album)
        {
            if (App.Navigator.CurrentPage is CollectionAlbumPage && album.Songs.Count == 0)
            {
                App.Navigator.GoBack();
            }
            ExitIfArtistEmpty(album.PrimaryArtist);
        }

        #region Command Execute

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

        #region Navigatings

        private void AlbumClickExecute(ItemClickEventArgs obj)
        {
            var album = obj.ClickedItem as Album;
            App.Navigator.GoTo<CollectionAlbumPage, ZoomInTransition>(album.Id);
        }

        private void ArtistClickExecute(ItemClickEventArgs obj)
        {
            var artist = obj.ClickedItem as Artist;
            App.Navigator.GoTo<CollectionArtistPage, ZoomInTransition>(artist.Id);
        }

        private void PlaylistClickExecute(ItemClickEventArgs obj)
        {
            var playlist = obj.ClickedItem as Playlist;
            App.Navigator.GoTo<CollectionPlaylistPage, ZoomInTransition>(playlist.Id);
        }

        #endregion

        #region Downloading

        private void CancelClickExecute(Song song)
        {
            if (song.Download != null)
                _downloadService.Cancel(song.Download);
            else
            {
                song.SongState = SongState.None;
                _sqlService.UpdateItemAsync(song);
            }
        }

        private void DownloadClickExecute(Song song)
        {
            _downloadService.StartDownloadAsync(song);
        }

        #endregion

        #region Queue

        private void AddToClickExecute(BaseEntry entry)
        {
            _addable = new List<Song>();

            if (entry is Song)
            {
                var song = entry as Song;
                song.AddableTo.Clear();
                song.AddableTo.Add(new AddableCollectionItem
                {
                    Name = "NowPlayingName".FromLanguageResource()
                });
                song.AddableTo.AddRange(_service
                    .Playlists.Where(p => p.Songs.Count(pp => pp.SongId == song.Id) == 0)
                    .Select(p => new AddableCollectionItem
                    {
                        Playlist = p,
                        Name = p.Name
                    }));
                _addable.Add(song);
            }
            else if (entry is Album)
            {
                var album = entry as Album;
                var albumSongs = album.Songs.ToList();

                //when inserting we need to reverse the list to keep the order
                if (App.Locator.Settings.AddToInsert && App.Locator.Player.CurrentQueue != null)
                    albumSongs.Reverse();

                album.AddableTo.Clear();
                album.AddableTo.Add(new AddableCollectionItem
                {
                    Name = "NowPlayingName".FromLanguageResource()
                });

                album.AddableTo.AddRange(_service
                    .Playlists.Where(p => p.Songs.Count(pp => 
                        albumSongs.FirstOrDefault(t => t.Id == pp.SongId) != null) == 0)
                    .Select(p => new AddableCollectionItem
                    {
                        Playlist = p,
                        Name = p.Name
                    }));
                _addable.AddRange(albumSongs);
            }

            else if (entry is Artist)
            {
                var artist = entry as Artist;
                var artistSongs = artist.Songs.ToList();

                //when inserting we need to reverse the list to keep the order
                if (App.Locator.Settings.AddToInsert && App.Locator.Player.CurrentQueue != null)
                    artistSongs.Reverse();

                artist.AddableTo.Clear();
                artist.AddableTo.Add(new AddableCollectionItem
                {
                    Name = "NowPlayingName".FromLanguageResource()
                });

                artist.AddableTo.AddRange(_service
                    .Playlists.Where(p => p.Songs.Count(pp =>
                        artistSongs.FirstOrDefault(t => t.Id == pp.SongId) != null) == 0)
                    .Select(p => new AddableCollectionItem
                    {
                        Playlist = p,
                        Name = p.Name
                    }));
                _addable.AddRange(artistSongs);
            }
        }

        private List<Song> _addable;

        private async void ItemPickedExecute(AddableCollectionItem item)
        {
            for (var i = 0; i < _addable.Count; i++)
            {
                var song = _addable[i];
                var playIfNotActive = i == (App.Locator.Settings.AddToInsert 
                && App.Locator.Player.CurrentQueue != null ? _addable.Count - 1 : 0);

                if (item.Playlist != null)
                {
                    //only add if is not there already
                    if (item.Playlist.Songs.FirstOrDefault(p => p.SongId == song.Id) == null)
                        await _service.AddToPlaylistAsync(item.Playlist, song).ConfigureAwait(false);
                }

                else
                    //the last song insert it into the shuffle list (the others shuffle them around)
                    await CollectionHelper.AddToQueueAsync(song, i == _addable.Count - 1, 
                        playIfNotActive, i == 0).ConfigureAwait(false);

                if (App.Locator.Player.CurrentQueue != null || !App.Locator.Settings.AddToInsert) continue;

                _addable.RemoveAt(0);
                _addable.Reverse();
                _addable.Insert(0, song);
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

                    await _service.DeleteSongAsync(song);
                    ExitIfAlbumEmpty(song.Album);
                }

                else if (item is Playlist)
                {
                    var playlist = item as Playlist;
                    name = playlist.Name;

                    await _service.DeletePlaylistAsync(playlist);
                }

                else if (item is Artist)
                {
                    var artist = item as Artist;
                    name = artist.Name;

                    _service.Artists.Remove(artist);

                    await Task.WhenAll(artist.Songs.ToList().Select(song => Task.WhenAll(new List<Task>
                    {
                        _service.DeleteSongAsync(song)
                    })));

                    ExitIfArtistEmpty(artist);
                }

                else if (item is Album)
                {
                    var album = item as Album;
                    name = album.Name;

                    _service.Albums.Remove(album);

                    await Task.WhenAll(album.Songs.ToList().Select(song => Task.WhenAll(new List<Task>
                    {
                        _service.DeleteSongAsync(song)
                    })));

                    ExitIfAlbumEmpty(album);
                }

                CurtainPrompt.Show("EntryDeletingSuccess".FromLanguageResource(), name);
            }
            catch
            {
                CurtainPrompt.ShowError("EntryDeletingError".FromLanguageResource(), name);
            }
        }

        #endregion

        #endregion

        #region Commands

        public RelayCommand<ItemClickEventArgs> PlaylistClickCommand { get; set; }

        public RelayCommand<ItemClickEventArgs> AlbumClickCommand { get; set; }

        public RelayCommand<ItemClickEventArgs> ArtistClickCommand { get; set; }


        public RelayCommand<Song> CancelClickCommand { get; set; }

        public RelayCommand<Song> DownloadClickCommand { get; set; }


        public RelayCommand<BaseEntry> AddToClickCommand { get; set; }

        public RelayCommand<BaseEntry> DeleteClickCommand { get; set; }

        public RelayCommand<AddableCollectionItem> ItemPickedCommand { get; set; }

        public RelayCommand<BaseEntry> EntryPlayClickCommand { get; set; }

        #endregion
    }
}
