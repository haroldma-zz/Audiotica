using System;
using System.Threading.Tasks;

using Audiotica.Core.Utils.Interfaces;

namespace Audiotica.iOS.Implementations
{
    internal class DispatcherHelper : IDispatcherHelper
    {
        public Task RunAsync(Action action)
        {
            var uiSource = new TaskCompletionSource<bool>();
            AppDelegate.Current.BeginInvokeOnMainThread(
                () =>
                {
                    try
                    {
                        action();
                    }
                    catch
                    {
                        // ignored
                    }

                    uiSource.SetResult(true);
                });
            return uiSource.Task;
        }
    }
}