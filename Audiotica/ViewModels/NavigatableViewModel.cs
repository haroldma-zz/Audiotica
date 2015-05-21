using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Universal.Mvvm;
using Audiotica.Services.NavigationService;

namespace Audiotica.ViewModels
{
    public abstract class NavigatableViewModel : ViewModelBase, INavigatable
    {
        public virtual void OnNavigatedTo(object parameter, NavigationMode mode, Dictionary<string, object> state)
        {
        }

        public virtual void OnNavigatedFrom(bool suspending, Dictionary<string, object> state)
        {
        }
    }
}