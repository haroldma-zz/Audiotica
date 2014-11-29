#region

using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Audiotica.Core.Common;
using Newtonsoft.Json.Linq;

#endregion

namespace Audiotica.Core.Utilities
{
    public static class BetaChangelogHelper
    {
        public static AppVersion CurrentVersion
        {
            get { return Package.Current.Id.Version; }
        }

        public static bool IsFirstRun { get; private set; }

        public static bool JustUpdated { get; private set; }

        public static async Task OnLaunchedAsync()
        {
            var changelog = await GetChangelogAsync();

            var previousVersion = AppSettingsHelper.ReadJsonAs<AppVersion>("LastRunVersion");
            
            IsFirstRun = previousVersion == null;

            if (IsFirstRun)
                await MessageBox.ShowAsync(changelog.FirstRunMessage, 
                    "Welcome to " + CurrentVersion.ReleaseNumber + " - Beta " + CurrentVersion.BetaNumber);
            else
            {
// ReSharper disable once PossibleNullReferenceException
                JustUpdated = previousVersion.CompareTo(CurrentVersion) == -1;

                if (JustUpdated)
                    await MessageBox.ShowAsync(changelog.JustUpdatedMessage, "Just Updated\nBeta " + CurrentVersion.BetaNumber + " - Patch #" + CurrentVersion.PatchNumber);
            }

            if (IsFirstRun || JustUpdated)
                AppSettingsHelper.WriteAsJson("LastRunVersion", CurrentVersion);
        }

        private static async Task<dynamic> GetChangelogAsync()
        {
            var file = await StorageHelper.GetFileAsync("Assets/BetaChangelog.xml", StorageHelper.StorageStrategy.Installation);
            var stream = await file.OpenStreamForReadAsync();
            var xDoc = XDocument.Load(stream);
            dynamic expando = new ExpandoObject();

            var changelogNode = xDoc.Element("Changelog");

            const string tab = "\n    ";
            expando.FirstRunMessage = changelogNode.Element("FirstRunMessage").Value.Trim().Replace(tab, "\n");
            expando.JustUpdatedMessage = changelogNode.Element("JustUpdatedMessage").Value.Trim().Replace(tab, "\n");

            return expando;
        }
    }
}