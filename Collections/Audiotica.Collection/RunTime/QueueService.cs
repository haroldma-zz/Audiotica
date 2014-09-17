using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Audiotica.Collection.Model;

namespace Audiotica.Collection.RunTime
{
    public class QueueService : IQueueService
    {
        private readonly ISqlService _sqlService;
        private readonly ICollectionService _collectionService;
        private readonly Dictionary<int, QueueSong> _lookupMap = new Dictionary<int, QueueSong>();

        public QueueService(ISqlService sqlService, ICollectionService collectionService)
        {
            _sqlService = sqlService;
            _collectionService = collectionService;
        }

        public ObservableCollection<QueueSong> PlaybackQueue { get; private set; }

        public void LoadQueue()
        {
            PlaybackQueue = new ObservableCollection<QueueSong>();
            var queue = _sqlService.Connection.Table<QueueSong>().ToList();
            QueueSong head = null;

            foreach (var queueSong in queue)
            {
                queueSong.Song = _collectionService.Songs.FirstOrDefault(p => p.Id == queueSong.SongId);

                _lookupMap.Add(queueSong.Id, queueSong);
                if (queueSong.PrevId == 0)
                    head = queueSong;
            }

            if (head == null)
                return;

            for (var i = 0; i < queue.Count; i++)
            {
                PlaybackQueue.Add(head);

                if (head.NextId != 0)
                    head = _lookupMap[head.NextId];
            }
        }

        public Task LoadQueueAsync()
        {
            return Task.Factory.StartNew(LoadQueue);
        }

        public async Task ClearQueueAsync()
        {
            if (PlaybackQueue.Count == 0) return;

            await _sqlService.DeleteAllAsync<QueueSong>();
            _lookupMap.Clear();
            PlaybackQueue.Clear();
        }

        public async Task AddSongAsync(Song song)
        {
            var tail = PlaybackQueue.LastOrDefault();

            //Create the new queue entry
            var newQueue = new QueueSong { SongId = song.Id, NextId = 0, PrevId = tail == null ? 0 : tail.Id, Song = song};

            //Add it to the database
            await _sqlService.InsertAsync(newQueue);

            if (tail != null)
            {
                //Update the next id of the previous tail
                tail.NextId = newQueue.Id;
                await _sqlService.UpdateAsync(tail);
            }

            //Add the new queue entry to the collection and map
            PlaybackQueue.Add(newQueue);
            _lookupMap.Add(newQueue.Id, newQueue);
        }

        public Task MoveFromToAsync(int oldIndex, int newIndex)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(QueueSong queueSongToRemove)
        {
//            if (songToRemove.Entry.Id == CurrentPlaybackQueueEntryId)
//            {
//                Stop();
//                CurrentPlaybackQueueEntryId = 0;
//            }

            QueueSong previousModel = null;

            if (_lookupMap.TryGetValue(queueSongToRemove.PrevId, out previousModel))
            {
                previousModel.NextId = queueSongToRemove.NextId;
                await _sqlService.UpdateAsync(previousModel);
            }

            QueueSong nextModel = null;

            if (_lookupMap.TryGetValue(queueSongToRemove.NextId, out nextModel))
            {
                nextModel.PrevId = queueSongToRemove.PrevId;
                await _sqlService.UpdateAsync(nextModel);
            }

            PlaybackQueue.Remove(queueSongToRemove);
            _lookupMap.Remove(queueSongToRemove.Id);

            //Delete from database
            await _sqlService.DeleteAsync(queueSongToRemove);
        }
    }
}
