#region

using Windows.ApplicationModel;
using Audiotica.Core.Common;
using Audiotica.Core.Utils.Interfaces;

#endregion

namespace Audiotica.Core.WinRt.Utilities
{
    public class AppVersionHelper
    {
        private readonly IAppSettingsHelper _appSettingsHelper;

        public AppVersionHelper(IAppSettingsHelper settingsHelper)
        {
            _appSettingsHelper = settingsHelper;
        }

        public AppVersion CurrentVersion
        {
            get { return Package.Current.Id.Version; }
        }

        public bool IsFirstRun { get; private set; }
        public bool JustUpdated { get; private set; }

        public void OnLaunched()
        {
            var previousVersion = _appSettingsHelper.ReadJsonAs<AppVersion>("LastRunVersion");

            IsFirstRun = previousVersion == null;

            if (!IsFirstRun)
            {
                JustUpdated = previousVersion.CompareTo(CurrentVersion) == -1;
            }

            if (_appSettingsHelper.Read<bool>("SimulateFirstRun"))
                IsFirstRun = true;
            else if (_appSettingsHelper.Read<bool>("SimulateUpdate"))
                JustUpdated = true;
            _appSettingsHelper.WriteAsJson("LastRunVersion", CurrentVersion);
        }
    }
}