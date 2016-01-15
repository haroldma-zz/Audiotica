using Audiotica.Windows.Services.Interfaces;
using Autofac;

namespace Audiotica.Windows.Engine.Bootstrppers
{
    public class DownloadServiceBootstrapper : AppBootStrapper
    {
        public override void OnStart(IComponentContext context)
        {
            var downloadService = context.Resolve<IDownloadService>();
            downloadService.LoadDownloads();
        }
    }
}