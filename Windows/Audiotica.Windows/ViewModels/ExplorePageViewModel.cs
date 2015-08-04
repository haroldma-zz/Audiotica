using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Windows.Services;
using Audiotica.Database.Models;
using Audiotica.Web.MatchEngine.Interfaces.Providers;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Metadata.Providers;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    internal class ExplorePageViewModel : ViewModelBase
    {
        public ExplorePageViewModel()
        {
        }

        public override void OnNavigatedTo(object parameter, NavigationMode mode, Dictionary<string, object> state)
        {
        }
    }
}