using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Navigation;
using Audiotica.Web.Interfaces.MatchEngine;

namespace Audiotica.ViewModels
{
    internal class MainViewModel : NavigatableViewModel
    {
        private readonly IEnumerable<IProvider> _providers;

        public MainViewModel(IEnumerable<IProvider> providers)
        {
            _providers = providers.OrderByDescending(p => p.Speed + (int)p.ResultsQuality).ToList();
            // TODO
        }

        public override async void OnNavigatedTo(object parameter, NavigationMode mode, Dictionary<string, object> state)
        {
            foreach (var provider in _providers)
            {
                try
                {
                    var url = await provider.GetLinkAsync("FourFiveSeconds", "Rihanna");
                    if (url != null)
                    {
                    }
                }
                catch (ArgumentNullException)
                {
                    
                }
            }
        }
    }
}