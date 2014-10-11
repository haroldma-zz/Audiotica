#region

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
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
        private readonly IQueueService _queueService;
#if WINDOWS_PHONE_APP
        private readonly AudioPlayerHelper _audioPlayer;
#endif
        private readonly RelayCommand<ItemClickEventArgs> _songClickCommand;

        public CollectionViewModel(ICollectionService service, IQueueService queueService
#if WINDOWS_PHONE_APP
, AudioPlayerHelper audioPlayer
#endif
            )
        {
            _service = service;
            _queueService = queueService;
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

            await _queueService.ClearQueueAsync();

            foreach (var queueSong in _service.Songs)
            {
                await _queueService.AddSongAsync(queueSong);
            }

#if WINDOWS_PHONE_APP
            //play the song here
            _audioPlayer.PlaySong(song.Id);
#endif
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