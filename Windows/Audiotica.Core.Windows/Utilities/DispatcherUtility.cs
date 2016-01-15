using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Audiotica.Core.Utilities.Interfaces;

namespace Audiotica.Core.Windows.Utilities
{
    // DOCS: https://github.com/Windows-XAML/Template10/wiki/Docs-%7C-DispatcherWrapper
    public class DispatcherUtility : IDispatcherUtility
    {
        private readonly CoreDispatcher _dispatcher;

        public DispatcherUtility(CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public bool HasThreadAccess() => _dispatcher.HasThreadAccess;

        public async void Run(Action action, int delayms = 0)
        {
            await Task.Delay(delayms);
            if (_dispatcher.HasThreadAccess)
            {
                action();
            }
            else
            {
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).AsTask().Wait();
            }
        }

        public T Run<T>(Func<T> action, int delayms = 0) where T : class
        {
            Task.Delay(delayms);
            if (_dispatcher.HasThreadAccess)
            {
                return action();
            }
            T result = null;
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => result = action()).AsTask().Wait();
            return result;
        }

        public async Task RunAsync(Action action, int delayms = 0)
        {
            await Task.Delay(delayms);
            if (_dispatcher.HasThreadAccess)
            {
                action();
            }
            else
            {
                var tcs = new TaskCompletionSource<object>();
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                        {
                            try
                            {
                                action();
                                tcs.TrySetResult(null);
                            }
                            catch (Exception ex)
                            {
                                tcs.TrySetException(ex);
                            }
                        });
                await tcs.Task;
            }
        }

        public async Task RunAsync(Func<Task> func, int delayms = 0)
        {
            await Task.Delay(delayms);
            if (_dispatcher.HasThreadAccess)
            {
                await func?.Invoke();
            }
            else
            {
                var tcs = new TaskCompletionSource<object>();
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    async () =>
                        {
                            try
                            {
                                await func();
                                tcs.TrySetResult(null);
                            }
                            catch (Exception ex)
                            {
                                tcs.TrySetException(ex);
                            }
                        });
                await tcs.Task;
            }
        }

        public async Task<T> RunAsync<T>(Func<T> func, int delayms = 0)
        {
            await Task.Delay(delayms);
            if (_dispatcher.HasThreadAccess)
            {
                return func();
            }
            var tcs = new TaskCompletionSource<T>();
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                    {
                        try
                        {
                            tcs.TrySetResult(func());
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                        }
                    });
            await tcs.Task;
            return tcs.Task.Result;
        }
    }
}