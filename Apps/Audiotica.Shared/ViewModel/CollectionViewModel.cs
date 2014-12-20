#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using Windows.Globalization.Collation;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

#endregion

namespace Audiotica.ViewModel
{
    public class CollectionViewModel : ViewModelBase
    {
        private readonly ICollectionService _service;
#if WINDOWS_PHONE_APP
        private readonly AudioPlayerHelper _audioPlayer;
#endif
        private readonly RelayCommand<ItemClickEventArgs> _songClickCommand;
        private ObservableCollection<AlphaKeyGroup<Song>> _sortedSongs;

        public CollectionViewModel(ICollectionService service
#if WINDOWS_PHONE_APP
, AudioPlayerHelper audioPlayer
#endif
            )
        {
            _service = service;
#if WINDOWS_PHONE_APP
            _audioPlayer = audioPlayer;
#endif

            _songClickCommand = new RelayCommand<ItemClickEventArgs>(SongClickExecute);

            if (IsInDesignModeStatic)
                _service.LoadLibrary();

            SortedSongs = AlphaKeyGroup<Song>.CreateGroups(Service.Songs, 
                CultureInfo.CurrentCulture, item => item.Name, true);
            Service.Songs.CollectionChanged += SongsOnCollectionChanged;
        }

        private void SongsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs arg)
        {
            var zero = false;
            AlphaKeyGroup<Song> group = null;

            switch (arg.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    var song = arg.NewItems[0] as Song;
                    group = SortedSongs.First(a => a.Key == new CharacterGroupings().Lookup(song.Name));
                    zero = group.Count == 0;
                    group.Items.Add(song);
                }
                    break;
                case NotifyCollectionChangedAction.Remove:
                {
                    var song = arg.OldItems[0] as Song;
                    group = SortedSongs.First(a => a.Key == new CharacterGroupings().Lookup(song.Name));
                    group.Items.Remove(song);
                    zero = group.Count == 0;
                }
                    break;
            }

            if (!zero) return;

            var index = SortedSongs.IndexOf(@group);
            SortedSongs.Remove(@group);
            SortedSongs.Insert(index, @group);
        }

        private async void SongClickExecute(ItemClickEventArgs e)
        {
            var song = e.ClickedItem as Song;

            await _service.ClearQueueAsync();

            foreach (var queueSong in _service.Songs)
            {
                await _service.AddToQueueAsync(queueSong);
            }

#if WINDOWS_PHONE_APP
            _audioPlayer.PlaySong(_service.PlaybackQueue[_service.Songs.IndexOf(song)]);
#endif
        }

        private int rowCount
        {
            get
            {
                //extra column if running on hd device (720 and 1080)
                var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
                var actualWidth = (int)(Window.Current.Bounds.Width * scaleFactor);
                return actualWidth == 720 || actualWidth == 1080 ? 5 : 4;
            }
        }

        public ObservableCollection<AlphaKeyGroup<Song>> SortedSongs
        {
            get
            {
                return _sortedSongs;
            }
            set
            {
                Set(ref _sortedSongs, value);
            }
        }

        public List<Album> RandomizeAlbumList
        {
            get
            {
                var albums = Service.Albums.Where(p => p.Artwork != CollectionConstant.MissingArtworkImage).ToList();

                var albumCount = albums.Count;

                if (albumCount == 0) return null;

                var h = IsInDesignMode ? 800 : Window.Current.Bounds.Height;
                var rows = (int)Math.Ceiling(h / ArtworkSize);

                var numImages = rows * rowCount;
                var imagesNeeded = numImages - albumCount;

                var shuffle = albums
                    .Shuffle()
                    .Take(numImages > albumCount ? albumCount : numImages)
                    .ToList();

                if (imagesNeeded <= 0) return shuffle;

                var repeatList = new List<Album>();

                while (imagesNeeded > 0)
                {
                    var takeAmmount = imagesNeeded > albumCount ? albumCount : imagesNeeded;
                    
                    repeatList.AddRange(shuffle.Shuffle().Take(takeAmmount));

                    imagesNeeded -= shuffle.Count;
                }

                shuffle.AddRange(repeatList);

                return shuffle;
            }
        }

        public double ArtworkSize
        {
            get
            {
                var w = IsInDesignMode ? 480 : Window.Current.Bounds.Width;
                return w / rowCount;
            }
        }

        public ICollectionService Service
        {
            get { return _service; }
        }

        public RelayCommand<ItemClickEventArgs> SongClickCommand
        {
            get { return _songClickCommand; }
        }
    }
}