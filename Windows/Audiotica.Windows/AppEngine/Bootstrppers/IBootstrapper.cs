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
        void OnLaunched(IComponentContext context);

        /// <summary>
        ///     Called when [relaunched].
        ///     BootStrapper should restore it's previous state, to create the illusion it was resumed.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="state">The previous state of the module.</param>
        /// <returns></returns>
        void OnRelaunched(IComponentContext context, Dictionary<string, object> state);

        /// <summary>
        ///     Called when [resuming].
        ///     BootStrapper should refresh any stale data (network, etc).
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        void OnResuming(IComponentContext context);

        /// <summary>
        ///     Called when [suspending].
        ///     BootStrapper should disposed of any exclusive resource or file handled.
        ///     Along with saving the current state, in cased the app is terminated.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="state">The previous state of the module.</param>
        /// <returns></returns>
        void OnSuspending(IComponentContext context, Dictionary<string, object> state);
    }

    public class AppBootStrapper : IBootStrapper
    {
        public virtual void OnLaunched(IComponentContext context)
        {
        }

        public virtual void OnRelaunched(IComponentContext context, Dictionary<string, object> state)
        {
        }

        public virtual void OnResuming(IComponentContext context)
        {
        }

        public virtual void OnSuspending(IComponentContext context, Dictionary<string, object> state)
        {
        }
    }
}