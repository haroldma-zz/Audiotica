using System.Collections.Generic;
using System.Threading.Tasks;

namespace Audiotica.Windows.AppEngine.Bootstrppers
{
    public interface IBootStrapper
    {
        /// <summary>
        ///     Called when the app is [launched].
        ///     BootStrapper should initiate from scratch and disposed of any saved state.
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        /// <returns></returns>
        Task OnLaunchedAsync(AppKernel kernel);

        /// <summary>
        /// Called when [relaunched].
        /// BootStrapper should restore it's previous state, to create the illusion it was resumed.
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        /// <param name="state">The previous state of the module.</param>
        /// <returns></returns>
        Task OnRelaunchedAsync(AppKernel kernel, Dictionary<string, object> state);

        /// <summary>
        ///     Called when [resuming].
        ///     BootStrapper should refresh any stale data (network, etc).
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        /// <returns></returns>
        Task OnResumingAsync(AppKernel kernel);

        /// <summary>
        ///     Called when [suspending].
        ///     BootStrapper should disposed of any exclusive resource or file handled.
        ///     Along with saving the current state, in cased the app is terminated.
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        /// <param name="state">The previous state of the module.</param>
        /// <returns></returns>
        Task OnSuspendingAsync(AppKernel kernel, Dictionary<string, object> state);
    }

    public class AppBootStrapper : IBootStrapper
    {
        public virtual Task OnLaunchedAsync(AppKernel kernel)
        {
            return Task.FromResult(0);
        }

        public virtual Task OnRelaunchedAsync(AppKernel kernel, Dictionary<string, object> state)
        {
            return Task.FromResult(0);
        }

        public virtual Task OnResumingAsync(AppKernel kernel)
        {
            return Task.FromResult(0);
        }

        public virtual Task OnSuspendingAsync(AppKernel kernel, Dictionary<string, object> state)
        {
            return Task.FromResult(0);
        }
    }
}