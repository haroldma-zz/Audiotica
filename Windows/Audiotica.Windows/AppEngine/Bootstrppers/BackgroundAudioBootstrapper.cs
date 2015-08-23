using System.Collections.Generic;
using System.Threading.Tasks;
using Audiotica.Core.Windows.Services;
using Autofac;

namespace Audiotica.Windows.AppEngine.Bootstrppers
{
    public class BackgroundAudioBootstrapper : AppBootStrapper
    {
        protected Task StartTaskAsync(IComponentContext context)
        {
            var service = context.Resolve<IPlayerService>();
            return service.StartBackgroundTaskAsync();
        }

        public override Task OnLaunchedAsync(IComponentContext context)
        {
            return StartTaskAsync(context);
        }

        public override Task OnRelaunchedAsync(IComponentContext context, Dictionary<string, object> state)
        {
            return StartTaskAsync(context);
        }

        public override Task OnResumingAsync(IComponentContext context)
        {
            var service = context.Resolve<IPlayerService>();

            // Tell the background audio that the app is being resumed
            service.Resuming();

            return base.OnResumingAsync(context);
        }

        public override Task OnSuspendingAsync(IComponentContext context, Dictionary<string, object> state)
        {
            var service = context.Resolve<IPlayerService>();

            // Tell the background audio that the app is being suspended
            service.Suspending();

            return base.OnSuspendingAsync(context, state);
        }
    }
}