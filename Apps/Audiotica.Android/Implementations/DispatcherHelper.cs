using System;
using System.Threading.Tasks;
using Audiotica.Core.Utils.Interfaces;

namespace Audiotica.Android.Implementations
{
    internal class DispatcherHelper : IDispatcherHelper
    {
        public Task RunAsync(Action action)
        {
            var taskCompletion = new TaskCompletionSource<bool>();
            App.Current.CurrentActivity.RunOnUiThread(() =>
            {
                try
                {
                    action();
                }
                catch
                {
                    // ignored
                }
                taskCompletion.SetResult(true);
            });
            return taskCompletion.Task;
        }
    }
}