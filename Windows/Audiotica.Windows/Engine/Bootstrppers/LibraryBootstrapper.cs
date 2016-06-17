using System;
using System.Linq;
using Audiotica.Core.Extensions;
using Audiotica.Core.Windows.Helpers;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Web.Services;
using Audiotica.Windows.Services.Interfaces;
using Autofac;

namespace Audiotica.Windows.Engine.Bootstrppers
{
    public class LibraryBootstrapper : AppBootStrapper
    {
        public override void OnStart(IComponentContext context)
        {
            var service = context.Resolve<ILibraryService>();
            var insights = context.Resolve<IAnalyticService>();

            using (var timer = insights.TrackTimeEvent("Library Loaded"))
            {
                service.Load();
                timer.AddProperty("track count", service.Tracks.Count.ToString());
            }


            var matchingService = context.Resolve<ILibraryMatchingService>();
            matchingService.OnStartup();

            CleanupFiles(s => !service.Tracks.Any(p => p.ArtistArtworkUri?.EndsWithIgnoreCase(s) ?? false), "Library/Images/Artists/");
            CleanupFiles(s => !service.Tracks.Any(p => p.ArtworkUri?.EndsWithIgnoreCase(s) ?? false), "Library/Images/Albums/");
        }
        
        private static async void CleanupFiles(Func<string, bool> shouldDelete, string folderPath)
        {
            var folder = await StorageHelper.GetFolderAsync(folderPath);
            if (folder == null) return;
            var files = await folder.GetFilesAsync();
            foreach (var file in files.Where(file => shouldDelete(file.Name)))
                await file.DeleteAsync();
        }
    }
}