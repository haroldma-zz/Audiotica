using System.Collections.Generic;
using System.Threading.Tasks;

namespace Audiotica.AppEngine.Bootstrppers
{
    internal class CollectionBootStrapper : AppBootStrapper
    {
        public override Task OnLaunchedAsync(AppKernel kernel)
        {
            return base.OnLaunchedAsync(kernel);
        }

        public override Task OnRelaunchedAsync(AppKernel kernel, Dictionary<string, object> state)
        {
            return base.OnRelaunchedAsync(kernel, state);
        }

        public override Task OnResumingAsync(AppKernel kernel)
        {
            return base.OnResumingAsync(kernel);
        }

        public override Task OnSuspendingAsync(AppKernel kernel, Dictionary<string, object> state)
        {
            return base.OnSuspendingAsync(kernel, state);
        }
    }
}