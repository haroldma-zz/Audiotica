using System.Collections.Generic;
using System.Threading.Tasks;
using Audiotica.Core.Windows.Services;

namespace Audiotica.Windows.AppEngine.Bootstrppers
{
    public class BackgroundAudioBootstrapper : AppBootStrapper
    {
        protected Task StartTaskAsync(AppKernel kernel)
        {
            var service = kernel.Resolve<IBackgroundAudioService>();
            return service.StartBackgroundTaskAsync();
        }

        public override Task OnLaunchedAsync(AppKernel kernel)
        {
            return StartTaskAsync(kernel);
        }

        public override Task OnRelaunchedAsync(AppKernel kernel, Dictionary<string, object> state)
        {
            return StartTaskAsync(kernel);
        }

        public override Task OnResumingAsync(AppKernel kernel)
        {
            var service = kernel.Resolve<IBackgroundAudioService>();

            // Tell the background audio that the app is being resumed
            service.Resuming();

            return base.OnResumingAsync(kernel);
        }

        public override Task OnSuspendingAsync(AppKernel kernel, Dictionary<string, object> state)
        {
            var service = kernel.Resolve<IBackgroundAudioService>();

            // Tell the background audio that the app is being suspended
            service.Suspending();

            return base.OnSuspendingAsync(kernel, state);
        }
    }
}