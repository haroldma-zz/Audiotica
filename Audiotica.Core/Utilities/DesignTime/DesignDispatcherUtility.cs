using System;
using System.Threading.Tasks;
using Audiotica.Core.Utilities.Interfaces;

namespace Audiotica.Core.Utilities.DesignTime
{
    public class DesignDispatcherUtility : IDispatcherUtility
    {
        public void Run(Action action)
        {
            
        }

        public Task RunAsync(Action action)
        {
            return Task.FromResult(0);
        }

        public Task<T> RunAsync<T>(Func<T> func)
        {
            return Task.FromResult(default(T));
        }

        public Task<T> RunAsync<T>(Func<Task<T>> func)
        {
            return Task.FromResult(default(T));
        }
    }
}