using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase
    {
        public SettingsPageViewModel(IAppSettingsUtility appSettingsUtility)
        {
            AppSettingsUtility = appSettingsUtility;
        }

        public IAppSettingsUtility AppSettingsUtility { get; }
    }
}