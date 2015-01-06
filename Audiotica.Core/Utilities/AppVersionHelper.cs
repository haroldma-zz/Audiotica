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
    public static class AppVersionHelper
    {
        public static AppVersion CurrentVersion
        {
            get { return Package.Current.Id.Version; }
        }

        public static bool IsFirstRun { get; private set; }

        public static bool JustUpdated { get; private set; }

        public static void OnLaunched()
        {
            var previousVersion = AppSettingsHelper.ReadJsonAs<AppVersion>("LastRunVersion");
            
            IsFirstRun = previousVersion == null;

            if (!IsFirstRun)
            {
                JustUpdated = previousVersion.CompareTo(CurrentVersion) == -1;
            }

            if (IsFirstRun || JustUpdated)
                AppSettingsHelper.WriteAsJson("LastRunVersion", CurrentVersion);

            if (AppSettingsHelper.Read<bool>("SimulateFirstRun"))
                IsFirstRun = true;
            else if (AppSettingsHelper.Read<bool>("SimulateUpdate"))
                JustUpdated = true;
        }
    }
}