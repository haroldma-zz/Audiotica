using System.Collections.Generic;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Services.Interfaces;
using Autofac;

namespace Audiotica.Windows.AppEngine.Bootstrppers
{
    public class LibraryBootstrapper : AppBootStrapper
    {
        public override void OnStart(IComponentContext context)
        {
            var service = context.Resolve<ILibraryService>();
            var insights = context.Resolve<IInsightsService>();

            using (var timer = insights.TrackTimeEvent("LibraryLoaded"))
            {
                service.Load();
                timer.AddProperty("Track count", service.Tracks.Count.ToString());
            }


            var matchingService = context.Resolve<ILibraryMatchingService>();
            matchingService.OnStartup();
        }
    }
}