using System.Collections.Generic;
using System.Threading.Tasks;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Services.Interfaces;
using Autofac;

namespace Audiotica.Windows.AppEngine.Bootstrppers
{
    public class LibraryBootstrapper : AppBootStrapper
    {
        protected async Task StartAsync(IComponentContext context)
        {
            var service = context.Resolve<ILibraryService>();
            var insights = context.Resolve<IInsightsService>();

            using (var timer = insights.TrackTimeEvent("LibraryLoaded"))
            {
                await service.LoadAsync();
                timer.AddProperty("Track count", service.Tracks.Count.ToString());
            }


            var matchingService = context.Resolve<ILibraryMatchingService>();
            matchingService.OnStartup();
        }

        public override Task OnLaunchedAsync(IComponentContext context)
        {
            return StartAsync(context);
        }

        public override Task OnRelaunchedAsync(IComponentContext context, Dictionary<string, object> state)
        {
            return StartAsync(context);
        }
    }
}