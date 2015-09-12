using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Audiotica.Core.Extensions;
using Audiotica.Core.Windows.Helpers;
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

            CleanupFiles(s => !service.Tracks.Any(p => p.ArtistArtworkUri?.EndsWithIgnoreCase(s) ?? false), "Library/Images/Artists/");
            CleanupFiles(s => !service.Tracks.Any(p => p.ArtworkUri?.EndsWithIgnoreCase(s) ?? false), "Library/Images/Albums/");
        }
        
        private static async void CleanupFiles(Func<string, bool> shouldDelete, string folderPath)
        {
            var folder = await StorageHelper.GetFolderAsync(folderPath);
            var files = await folder.GetFilesAsync();
            foreach (var file in files.Where(file => shouldDelete(file.Name)))
            {
                await file.DeleteAsync();
            }
        }
    }
}