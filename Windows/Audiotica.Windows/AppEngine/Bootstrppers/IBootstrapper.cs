using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;

namespace Audiotica.Windows.AppEngine.Bootstrppers
{
    public interface IBootStrapper
    {
        /// <summary>
        ///     Called when the app is [launched].
        ///     BootStrapper should initiate from scratch and disposed of any saved state.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        Task OnLaunchedAsync(IComponentContext context);

        /// <summary>
        ///     Called when [relaunched].
        ///     BootStrapper should restore it's previous state, to create the illusion it was resumed.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="state">The previous state of the module.</param>
        /// <returns></returns>
        Task OnRelaunchedAsync(IComponentContext context, Dictionary<string, object> state);

        /// <summary>
        ///     Called when [resuming].
        ///     BootStrapper should refresh any stale data (network, etc).
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        Task OnResumingAsync(IComponentContext context);

        /// <summary>
        ///     Called when [suspending].
        ///     BootStrapper should disposed of any exclusive resource or file handled.
        ///     Along with saving the current state, in cased the app is terminated.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="state">The previous state of the module.</param>
        /// <returns></returns>
        Task OnSuspendingAsync(IComponentContext context, Dictionary<string, object> state);
    }

    public class AppBootStrapper : IBootStrapper
    {
        public virtual Task OnLaunchedAsync(IComponentContext context)
        {
            return Task.FromResult(0);
        }

        public virtual Task OnRelaunchedAsync(IComponentContext context, Dictionary<string, object> state)
        {
            return Task.FromResult(0);
        }

        public virtual Task OnResumingAsync(IComponentContext context)
        {
            return Task.FromResult(0);
        }

        public virtual Task OnSuspendingAsync(IComponentContext context, Dictionary<string, object> state)
        {
            return Task.FromResult(0);
        }
    }
}