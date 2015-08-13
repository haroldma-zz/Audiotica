using System.Collections.Generic;
using System.Threading.Tasks;
using Audiotica.Database.Services.Interfaces;

namespace Audiotica.Windows.AppEngine.Bootstrppers
{
    public class CollectionBootstrapper : AppBootStrapper
    {
        protected Task StartAsync(AppKernel kernel)
        {
            var service = kernel.Resolve<ILibraryService>();
            return service.LoadAsync();
        }

        public override Task OnLaunchedAsync(AppKernel kernel)
        {
            return StartAsync(kernel);
        }

        public override Task OnRelaunchedAsync(AppKernel kernel, Dictionary<string, object> state)
        {
            return StartAsync(kernel);
        }
    }
}