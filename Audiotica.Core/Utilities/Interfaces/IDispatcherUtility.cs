using System;
using System.Threading.Tasks;

namespace Audiotica.Core.Utilities.Interfaces
{
    public interface IDispatcherUtility
    {
        void Run(Action action, int delayms = 0);
        T Run<T>(Func<T> action, int delayms = 0) where T : class;
        Task RunAsync(Func<Task> func, int delayms = 0);
        Task RunAsync(Action action, int delayms = 0);
        Task<T> RunAsync<T>(Func<T> func, int delayms = 0);
        bool HasThreadAccess();
    }
}