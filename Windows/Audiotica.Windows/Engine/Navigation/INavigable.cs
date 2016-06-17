using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Web.Services;

namespace Audiotica.Windows.Engine.Navigation
{
    // DOCS: https://github.com/Windows-XAML/Template10/wiki/Docs-%7C-NavigationService
    public interface INavigable
    {
        void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state);
        Task OnSaveStateAsync(IDictionary<string, object> state, bool suspending);
        void OnSaveState(IDictionary<string, object> state, bool suspending);
        void OnNavigatingFrom(NavigatingEventArgs args);
        INavigationService NavigationService { get; set; }
        IDispatcherUtility Dispatcher { get; set; }
        IStateItems SessionState { get; set; }
        IAnalyticService AnalyticService { get; set; }
    }
}
