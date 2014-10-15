#region

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Audiotica.Data.Collection.Model;

#endregion

namespace Audiotica.Data.Collection
{
    public interface IQueueService
    {
        ObservableCollection<QueueSong> PlaybackQueue { get; }

        Task LoadQueueAsync();

        Task ClearQueueAsync();


        Task AddSongAsync(Song song);
        Task MoveFromToAsync(int oldIndex, int newIndex);
        Task DeleteAsync(Song songToRemove);
    }
}