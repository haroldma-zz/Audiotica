using System.Collections.Generic;
using Audiotica.Windows.Services.Interfaces;
using Autofac;

namespace Audiotica.Windows.AppEngine.Bootstrppers
{
    public class BackgroundAudioBootstrapper : AppBootStrapper
    {
        public override void OnStart(IComponentContext context)
        {
            var service = context.Resolve<IPlayerService>();
            service.StartBackgroundTask();
        }
        
        public override void OnResuming(IComponentContext context)
        {
            var service = context.Resolve<IPlayerService>();

            // Tell the background audio that the app is being resumed
            service.Resuming();
        }

        public override void OnSuspending(IComponentContext context, Dictionary<string, object> state)
        {
            var service = context.Resolve<IPlayerService>();

            // Tell the background audio that the app is being suspended
            service.Suspending();
        }
    }
}