using System.Collections.Generic;
using Audiotica.Windows.Services.Interfaces;
using Autofac;

namespace Audiotica.Windows.AppEngine.Bootstrppers
{
    public class BackgroundAudioBootstrapper : IBootStrapper
    {
        public void OnLaunched(IComponentContext context)
        {
            StartTask(context);
        }

        public void OnRelaunched(IComponentContext context, Dictionary<string, object> state)
        {
            StartTask(context);
        }

        public void OnResuming(IComponentContext context)
        {
            var service = context.Resolve<IPlayerService>();

            // Tell the background audio that the app is being resumed
            service.Resuming();
        }

        public void OnSuspending(IComponentContext context, Dictionary<string, object> state)
        {
            var service = context.Resolve<IPlayerService>();

            // Tell the background audio that the app is being suspended
            service.Suspending();
        }

        protected void StartTask(IComponentContext context)
        {
            var service = context.Resolve<IPlayerService>();
            service.StartBackgroundTask();
        }
    }
}