#region

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
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
            //play the song here
            _audioPlayer.PlaySong(song.Id);
#endif
        }

        private int rowCount
        {
            get
            {
                //extra collumn if running on hd device (720, 768 and 1080)
                var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
                var actualWidth = (int)(Window.Current.Bounds.Width * scaleFactor);
                return actualWidth >= 720 ? 5 : 4;
            }
        }

        public List<Album> RandomizeAlbumList
        {
            get
            {
                var albums = Service.Albums.Where(p => p.Artwork != CollectionConstant.MissingArtworkImage).ToList();

                var albumCount = albums.Count;

                if (albumCount == 0) return null;

                var h = Window.Current.Bounds.Height;
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
                var w = Window.Current.Bounds.Width;
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