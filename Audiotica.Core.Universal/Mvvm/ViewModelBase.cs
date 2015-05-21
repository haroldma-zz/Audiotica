using Windows.ApplicationModel;
using Audiotica.Core.Common;

namespace Audiotica.Core.Universal.Mvvm
{
    public class ViewModelBase : ObservableObject
    {
        public bool IsInDesignMode => DesignMode.DesignModeEnabled;
    }
}