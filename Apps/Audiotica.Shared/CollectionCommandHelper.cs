#region

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using Audiotica.View;
using GalaSoft.MvvmLight.Command;

#endregion

namespace Audiotica
{
    public class CollectionCommandHelper
    {
        private readonly ISongDownloadService _downloadService;
        private readonly ICollectionService _service;
        private readonly ISqlService _sqlService;

        public CollectionCommandHelper(ICollectionService service, ISqlService sqlService,
            ISongDownloadService downloadService)
        {
            _service = service;
            _sqlService = sqlService;
            _downloadService = downloadService;

            CreateCommand();
        }

        public RelayCommand<BaseEntry> AddToPlaylistCommand { get; set; }
        public RelayCommand<BaseEntry> AddToQueueCommand { get; set; }

        private void CreateCommand()
        {
            ArtistClickCommand = new RelayCommand<ItemClickEventArgs>(ArtistClickExecute);
            AlbumClickCommand = new RelayCommand<ItemClickEventArgs>(AlbumClickExecute);
            PlaylistClickCommand = new RelayCommand<ItemClickEventArgs>(PlaylistClickExecute);

            AddToQueueCommand = new RelayCommand<BaseEntry>(AddToQueueExecute);
            AddToPlaylistCommand = new RelayCommand<BaseEntry>(AddToPlaylistExecute);

            DeleteClickCommand = new RelayCommand<BaseEntry>(DeleteClickExecute);
            DownloadClickCommand = new RelayCommand<Song>(DownloadClickExecute);
            CancelClickCommand = new RelayCommand<Song>(CancelClickExecute);

            EntryPlayClickCommand = new RelayCommand<BaseEntry>(EntryPlayClickExecute);
        }

        private void AddToPlaylistExecute(BaseEntry baseEntry)
        {
            List<Song> songs;

            if (baseEntry is Artist)
                songs = (baseEntry as Artist).Songs.ToList();
            else
                songs = (baseEntry as Album).Songs.ToList();

            CollectionHelper.AddToPlaylistDialog(songs);
        }

        private async void AddToQueueExecute(BaseEntry baseEntry)
        {
            List<Song> songs;
            var ignoreInsertMode = true;

            if (baseEntry is Artist)
                songs = (baseEntry as Artist).Songs.ToList();
            else
                songs = (baseEntry as Album).Songs.ToList();

            if (App.Locator.Settings.AddToInsert && App.Locator.Player.IsPlayerActive)
            {
                songs.Reverse();
                ignoreInsertMode = false;
            }

            await CollectionHelper.AddToQueueAsync(songs, ignoreInsertMode);
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
                await CollectionHelper.PlaySongsAsync(queueSongs, forceClear: true);
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

        public async void DeleteClickExecute(BaseEntry item)
        {
            await CollectionHelper.DeleteEntryAsync(item);
        }

        #endregion

        #endregion

        #region Commands

        public RelayCommand<ItemClickEventArgs> PlaylistClickCommand { get; set; }

        public RelayCommand<ItemClickEventArgs> AlbumClickCommand { get; set; }

        public RelayCommand<ItemClickEventArgs> ArtistClickCommand { get; set; }


        public RelayCommand<Song> CancelClickCommand { get; set; }

        public RelayCommand<Song> DownloadClickCommand { get; set; }

        public RelayCommand<BaseEntry> DeleteClickCommand { get; set; }


        public RelayCommand<BaseEntry> EntryPlayClickCommand { get; set; }

        #endregion
    }
}