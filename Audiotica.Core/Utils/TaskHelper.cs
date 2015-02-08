using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Audiotica.Core.Utils
{
    /// <summary>
    /// Helps manage various tasks
    /// </summary>
    public static class TaskHelper
    {
        private const int MaxSimultaneousTasks = 200;

        private static readonly List<Task> CurrentTasks = new List<Task>();

        private static bool _isRunning;

        public static async void Enqueue(Task task)
        {
            CurrentTasks.Add(task);

            if (_isRunning)
            {
                return;
            }

            _isRunning = true;
            await WhenAll(CurrentTasks);
            _isRunning = false;

            CurrentTasks.Clear();
        }

        public static async Task WhenAll(List<Task> tasks)
        {
            var downloaded = 0;

            // Limit to only 20 simultaneous threads
            while (downloaded < tasks.Count)
            {
                var currentTasks = tasks.Skip(downloaded).Take(MaxSimultaneousTasks).ToList();
                await Task.WhenAll(currentTasks);
                downloaded += currentTasks.Count;
            }
        }
    }
}