using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Common;
using Audiotica.Windows.Services.NavigationService;

namespace Audiotica.Windows.Tools.Mvvm
{
    public abstract class ViewModelBase : ObservableObject, INavigatable
    {
        public bool IsInDesignMode => DesignMode.DesignModeEnabled;

        public virtual void OnNavigatedTo(object parameter, NavigationMode mode, Dictionary<string, object> state)
        {   
        }

        public virtual void OnNavigatedFrom(bool suspending, Dictionary<string, object> state)
        {
        }
    }
}