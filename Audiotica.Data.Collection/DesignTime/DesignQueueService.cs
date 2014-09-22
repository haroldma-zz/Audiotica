using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Audiotica.Data.Collection.Model;

namespace Audiotica.Data.Collection.DesignTime
{
    public class DesignQueueService : IQueueService
    {
        public ObservableCollection<QueueSong> PlaybackQueue { get; private set; }
        public Task LoadQueueAsync()
        {
            throw new NotImplementedException();
        }

        public Task ClearQueueAsync()
        {
            throw new NotImplementedException();
        }

        public Task AddSongAsync(Song song)
        {
            throw new NotImplementedException();
        }

        public Task MoveFromToAsync(int oldIndex, int newIndex)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(QueueSong queueSongToRemove)
        {
            throw new NotImplementedException();
        }
    }
}
