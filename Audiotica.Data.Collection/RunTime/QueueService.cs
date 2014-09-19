#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Data.Collection.Model;

#endregion

namespace Audiotica.Data.Collection.RunTime
{
    public class QueueService : IQueueService
    {
        private readonly ICollectionService _collectionService;
        private readonly Dictionary<long, QueueSong> _lookupMap = new Dictionary<long, QueueSong>();
        private readonly ISqlService _sqlService;

        public QueueService(ISqlService sqlService, ICollectionService collectionService)
        {
            _sqlService = sqlService;
            _collectionService = collectionService;
        }

        public ObservableCollection<QueueSong> PlaybackQueue { get; private set; }

        public Task LoadQueueAsync()
        {
            return Task.Factory.StartNew(LoadQueue);
        }

        public async Task ClearQueueAsync()
        {
            if (PlaybackQueue.Count == 0) return;

            await _sqlService.DeleteTableAsync("QueueSong");
            _lookupMap.Clear();
            PlaybackQueue.Clear();
        }

        public async Task AddSongAsync(Song song)
        {
            var tail = PlaybackQueue.LastOrDefault();

            //Create the new queue entry
            var newQueue = new QueueSong
            {
                SongId = song.Id,
                NextId = 0,
                PrevId = tail == null ? 0 : tail.Id,
                Song = song
            };

            //Add it to the database
            await _sqlService.InsertQueueSongAsync(newQueue);

            if (tail != null)
            {
                //Update the next id of the previous tail
                tail.NextId = newQueue.Id;
                await _sqlService.UpdateQueueSongAsync(tail);
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
                await _sqlService.UpdateQueueSongAsync(previousModel);
            }

            QueueSong nextModel = null;

            if (_lookupMap.TryGetValue(queueSongToRemove.NextId, out nextModel))
            {
                nextModel.PrevId = queueSongToRemove.PrevId;
                await _sqlService.UpdateQueueSongAsync(nextModel);
            }

            PlaybackQueue.Remove(queueSongToRemove);
            _lookupMap.Remove(queueSongToRemove.Id);

            //Delete from database
            await _sqlService.DeleteItemAsync(queueSongToRemove.Id, "QueueSong");
        }

        public void LoadQueue()
        {
            PlaybackQueue = new ObservableCollection<QueueSong>();
            var queue = _sqlService.GetQueueSongs();
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
    }
}