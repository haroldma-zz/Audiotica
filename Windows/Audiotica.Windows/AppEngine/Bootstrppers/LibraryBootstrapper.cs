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
            var libraryMatching = context.Resolve<ILibraryMatchingService>();
            var insights = context.Resolve<IInsightsService>();

            using (var timer = insights.TrackTimeEvent("Loaded library"))
            {
                await service.LoadAsync();
                timer.AddProperty("Track count", service.Tracks.Count.ToString());
            }


            libraryMatching.OnStartup();
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